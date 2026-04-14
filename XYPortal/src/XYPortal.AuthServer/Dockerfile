# XYPortal.AuthServer Dockerfile
# 构建上下文必须为解决方案根目录: XYPortal/
# 例如: docker build -f src/XYPortal.AuthServer/Dockerfile -t xyportal-authserver:latest .

# ============================================
# Stage 1: base —运行时基础镜像
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 443

# ============================================
# Stage 2: build —编译构建环境
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# 安装 Node.js 20.x
RUN apt-get update && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# 安装 Yarn
RUN npm install -g yarn

# 安装 Volo.Abp.Studio.Cli
RUN dotnet tool install -g Volo.Abp.Studio.Cli

# 将全局 .NET 工具加入 PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# ============================================================
# 关键说明：
# - XYPortal.AuthServer.csproj 引用了 ../../common.props（解决方案根目录）
# - 它还通过相对路径引用 XYPortal.EntityFrameworkCore 项目
# - 因此构建上下文必须设为 XYPortal 解决方案根目录，
#   而非仅复制 AuthServer 子目录
# ============================================================
WORKDIR /src

# 复制解决方案文件（含 .slnx + common.props）
COPY XYPortal.slnx ./
COPY common.props ./

# 复制所有源码项目（AuthServer 依赖 EntityFrameworkCore 等）
COPY src/ ./src/
COPY test/ ./test/

# 还原 NuGet 包
RUN dotnet restore

# ============================================================
# 安装前端依赖并运行 abp install-libs
# abp install-libs 必须从 AuthServer 目录执行，
# 因为 abp.resourcemapping.js 位于 src/XYPortal.AuthServer/
# ============================================================
WORKDIR /src/src/XYPortal.AuthServer
RUN yarn install --frozen-lockfile || yarn install
RUN dotnet run --project . -- abp install-libs || true

# ============================================================
# 回到解决方案根目录执行发布
# ============================================================
WORKDIR /src
RUN dotnet publish src/XYPortal.AuthServer/XYPortal.AuthServer.csproj \
        -c Release \
        -o /app/publish \
        --no-restore

# ============================================
# Stage 3: final —运行时镜像
# ============================================
FROM base AS final
WORKDIR /app

# 复制发布产物（包含 Views、wwwroot、Pages 等）
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "XYPortal.AuthServer.dll"]
