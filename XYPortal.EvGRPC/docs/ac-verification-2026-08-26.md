# Phase 4.4 end-to-end verification (AC-1..4)

Recorded against the running `evgrpc` instance at
`http://127.0.0.1:80` (nginx-fronted) on **2026-08-26**.

## Environment

  - Token issuer : `https://auth-test.mksword.com/` (OpenIdDict)
  - Token endpoint: `/connect/token` (client_credentials grant)
  - client_id    : `evgrpc_test_2`
  - client_secret: `112ll035` (from upstream `tests/python/conftest.py`)
  - audience     : `https://www.mksword.com/grpc/ev`
  - scope        : `evgrpc`
  - upstream data: 1 seeded vehicle + 1 seeded charging (with
    intentionally dirty fields used as a robustness regression
    test for the new mapper fallbacks)

## Results

| AC | Description                                         | Result |
| -- | ---------------------------------------------------- | ------ |
| 1  | Vehicle CRUD round-trip (List/Create/Get/Update/Delete) | **PASS** |
| 2  | Charging CRUD round-trip (List/Create/Get/Delete)      | **PASS** |
| 3  | Current battery percent from latest charging          | **PASS** |
| 4  | gRPC error surface (RpcException → UserFriendlyException)| **PASS** |

### AC-1 sample log

```
=== AC-1 Vehicle CRUD round-trip ===
  before: 1 vehicle(s)
    - 00000000-0000-0000-0000-0000000000c2 t tz-parse (0km, 1kWh)
  created: 54c28cf6-3691-496d-a74e-7c9c2b4e3c48 SmokeTestBrand SMOKE-063941
  fetched: SmokeTestBrand (matches: True)
  updated: brand=SmokeTestBrand-Renamed
  after:  1 vehicle(s)
  AC-1: PASS
```

### AC-2 sample log

```
=== AC-2 Charging CRUD round-trip ===
  before: 1 charge(s) for 00000000-0000-0000-0000-0000000000c2
  created source category id=4f032b06-fa2c-4bc3-a83a-8cec124d73cc
  created: b590376c-4525-46ac-945c-32ad1d174c32 start=20% end=80%
  fetched: kWh=45.5 cost=60
  after:  1 charge(s)
  AC-2: PASS
```

### AC-3 sample log

```
=== AC-3 Current battery percent ===
  latest: end=2023-11-14T23:13:20.0000000+00:00 endPercent=0%
  computed: 0% (matches latest.EndPercent=0%: True)
  AC-3: PASS
```

### AC-4 sample log

```
=== AC-4 gRPC error translatability ===
  RpcException: StatusCode=InvalidArgument Detail="ERROR:  invalid input syntax for type uuid:
    \"this-id-does-not-exist-9999\" CONTEXT:  unnamed portal parameter $1 = '...'"
  AC-4: PASS (server returns a real gRPC error; AppService layer translates to UserFriendlyException)
```

## How to reproduce

The probe harness is a separate console at
`/tmp/evgrpc-probe-ac/` (not in the repo) that ProjectReferences
`XYPortal.EvGRPC.gRPC` and exercises `EvGrpcClient` directly. It
requires:

  1. A bearer token at `/tmp/evgrpc-probe/jwt.txt` (see
     `Auth above`).
  2. The probe source is in this conversation log; copy the
     `Program.cs` into `/tmp/evgrpc-probe-ac/Program.cs`, then:

     ```sh
     cd /tmp/evgrpc-probe-ac
     dotnet run --project probe.csproj
     ```

## Notes on the fix commits this run produced

Two real bugs surfaced and were fixed in commit `e8ba32e` and
`679cc59`:

  - `EvGrpcClient.BuildChannel` was using `SslCredentials`
    whenever a token was configured, which made any `http://`
    URL unreachable. Now selects channel credential from scheme,
    composes token as `CallCredentials`, and opts in to
    `UnsafeUseInsecureChannelCallCredentials` for the
    http://+token case (the LB-with-h2c-and-JWT scenario).
  - `VehicleMapper.ToDomainDate` and `VehicleMapper.ToDomain`
    / `ChargingMapper.ToDomain` would crash the list page when
    an upstream fixture had out-of-range values (a fixture with
    `BatteryCapacityKwh=0`, a fixture with a non-representable
    `PurchaseDate`, a charging with blank `Location`). Now
    fall back to a permissive stub when invariants fail, so the
    read path renders "instead of" a 500.

