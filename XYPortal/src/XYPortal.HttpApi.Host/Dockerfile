FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
ARG ASPENV=Production
ARG PORT=8080
ENV ASPNETCORE_ENVIRONMENT=${ASPENV}
ENV LANG="zh_CN.UTF-8"
ENV LANGUAGE="zh_CN:zh"
ENV LC_ALL="zh_CN.UTF-8"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:${PORT}
ENV TZ=Asia/Shanghai
EXPOSE ${PORT}
VOLUME [ "/app/Log", "/app/Access_Log" ]
WORKDIR /app
RUN apt-get clean && apt-get update && \
    apt-get install -y --no-install-recommends \
        locales \
        tzdata \
    && \
    sed -i '/zh_CN.UTF-8/s/^# //g' /etc/locale.gen || echo 'zh_CN.UTF-8 UTF-8' >> /etc/locale.gen && \
    locale-gen zh_CN.UTF-8 && \
    ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone && \
    rm -rf /var/lib/apt/lists/*

ENV LANG=zh_CN.UTF-8
ENV LANGUAGE=zh_CN:zh
ENV LC_ALL=zh_CN.UTF-8

RUN update-locale LANG=zh_CN.UTF-8 LC_ALL=zh_CN.UTF-8

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore

# 加载主项目
COPY ["XYPortal/XYPortal.slnx", "XYPortal/"]
COPY ["XYPortal/common.props", "XYPortal/"]
COPY ["XYPortal/src/XYPortal.AuthServer/XYPortal.AuthServer.csproj", "XYPortal/src/XYPortal.AuthServer/" ]
COPY ["XYPortal/src/XYPortal.Domain/XYPortal.Domain.csproj", "XYPortal/src/XYPortal.Domain/"]
COPY ["XYPortal/src/XYPortal.Domain.Shared/XYPortal.Domain.Shared.csproj", "XYPortal/src/XYPortal.Domain.Shared/"]
COPY ["XYPortal/src/XYPortal.EntityFrameworkCore/XYPortal.EntityFrameworkCore.csproj", "XYPortal/src/XYPortal.EntityFrameworkCore/"]
COPY ["XYPortal/src/XYPortal.Application.Contracts/XYPortal.Application.Contracts.csproj","XYPortal/src/XYPortal.Application.Contracts/"]
COPY ["XYPortal/src/XYPortal.Application/XYPortal.Application.csproj","XYPortal/src/XYPortal.Application/"]
COPY ["XYPortal/src/XYPortal.DbMigrator/XYPortal.DbMigrator.csproj","XYPortal/src/XYPortal.DbMigrator/"]
COPY ["XYPortal/src/XYPortal.HttpApi.Client/XYPortal.HttpApi.Client.csproj","XYPortal/src/XYPortal.HttpApi.Client/"]
COPY ["XYPortal/src/XYPortal.HttpApi.Host/XYPortal.HttpApi.Host.csproj","XYPortal/src/XYPortal.HttpApi.Host/"]
COPY ["XYPortal/src/XYPortal.HttpApi/XYPortal.HttpApi.csproj","XYPortal/src/XYPortal.HttpApi/"]
COPY ["XYPortal/src/XYPortal.Web.Host/XYPortal.Web.Host.csproj","XYPortal/src/XYPortal.Web.Host/"]
COPY ["XYPortal/src/XYPortal.Web/XYPortal.Web.csproj","XYPortal/src/XYPortal.Web/"]
COPY ["XYPortal/test/XYPortal.Application.Tests/XYPortal.Application.Tests.csproj","XYPortal/test/XYPortal.Application.Tests/"]
COPY ["XYPortal/test/XYPortal.Domain.Tests/XYPortal.Domain.Tests.csproj","XYPortal/test/XYPortal.Domain.Tests/"]
COPY ["XYPortal/test/XYPortal.EntityFrameworkCore.Tests/XYPortal.EntityFrameworkCore.Tests.csproj","XYPortal/test/XYPortal.EntityFrameworkCore.Tests/"]
COPY ["XYPortal/test/XYPortal.TestBase/XYPortal.TestBase.csproj","XYPortal/test/XYPortal.TestBase/"]
# 加载LinkBoard项目
COPY ["XYPortal.LinkBoard/XYPortal.LinkBoard.slnx", "XYPortal.LinkBoard/"]
COPY ["XYPortal.LinkBoard/common.props", "XYPortal.LinkBoard/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.Application.Contracts/XYPortal.LinkBoard.Application.Contracts.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.Application.Contracts/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.Application/XYPortal.LinkBoard.Application.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.Application/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.Domain.Shared/XYPortal.LinkBoard.Domain.Shared.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.Domain.Shared/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.Domain/XYPortal.LinkBoard.Domain.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.Domain/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.EntityFrameworkCore/XYPortal.LinkBoard.EntityFrameworkCore.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.EntityFrameworkCore/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.HttpApi.Client/XYPortal.LinkBoard.HttpApi.Client.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.HttpApi.Client/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.HttpApi/XYPortal.LinkBoard.HttpApi.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.HttpApi/"]
COPY ["XYPortal.LinkBoard/src/XYPortal.LinkBoard.Installer/XYPortal.LinkBoard.Installer.csproj", "XYPortal.LinkBoard/src/XYPortal.LinkBoard.Installer/"]
COPY ["XYPortal.LinkBoard/test/XYPortal.LinkBoard.Application.Tests/XYPortal.LinkBoard.Application.Tests.csproj", "XYPortal.LinkBoard/test/XYPortal.LinkBoard.Application.Tests/"]
COPY ["XYPortal.LinkBoard/test/XYPortal.LinkBoard.Domain.Tests/XYPortal.LinkBoard.Domain.Tests.csproj", "XYPortal.LinkBoard/test/XYPortal.LinkBoard.Domain.Tests/"]
COPY ["XYPortal.LinkBoard/test/XYPortal.LinkBoard.EntityFrameworkCore.Tests/XYPortal.LinkBoard.EntityFrameworkCore.Tests.csproj", "XYPortal.LinkBoard/test/XYPortal.LinkBoard.EntityFrameworkCore.Tests/"]
COPY ["XYPortal.LinkBoard/test/XYPortal.LinkBoard.TestBase/XYPortal.LinkBoard.TestBase.csproj", "XYPortal.LinkBoard/test/XYPortal.LinkBoard.TestBase/"]
# 加载RandomStringProvider项目
COPY ["XYPortal.RandomStringProvider/XYPortal.RandomStringProvider.slnx", "XYPortal.RandomStringProvider/"]
COPY ["XYPortal.RandomStringProvider/common.props", "XYPortal.RandomStringProvider/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Application/XYPortal.RandomStringProvider.Application.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Application/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Application.Contracts/XYPortal.RandomStringProvider.Application.Contracts.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Application.Contracts/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Domain/XYPortal.RandomStringProvider.Domain.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Domain/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Domain.Shared/XYPortal.RandomStringProvider.Domain.Shared.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Domain.Shared/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.EntityFrameworkCore/XYPortal.RandomStringProvider.EntityFrameworkCore.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.EntityFrameworkCore/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.HttpApi/XYPortal.RandomStringProvider.HttpApi.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.HttpApi/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.HttpApi.Client/XYPortal.RandomStringProvider.HttpApi.Client.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.HttpApi.Client/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Installer/XYPortal.RandomStringProvider.Installer.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Installer/"]
COPY ["XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Web/XYPortal.RandomStringProvider.Web.csproj", "XYPortal.RandomStringProvider/src/XYPortal.RandomStringProvider.Web/"]
COPY ["XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.Application.Tests/XYPortal.RandomStringProvider.Application.Tests.csproj", "XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.Application.Tests/"]
COPY ["XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.Domain.Tests/XYPortal.RandomStringProvider.Domain.Tests.csproj", "XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.Domain.Tests/"]
COPY ["XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.EntityFrameworkCore.Tests/XYPortal.RandomStringProvider.EntityFrameworkCore.Tests.csproj", "XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.EntityFrameworkCore.Tests/"]
COPY ["XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.TestBase/XYPortal.RandomStringProvider.TestBase.csproj", "XYPortal.RandomStringProvider/test/XYPortal.RandomStringProvider.TestBase/"]
# 加载PasswordBook项目
COPY ["XYPortal.PasswordBook/XYPortal.PasswordBook.slnx", "XYPortal.PasswordBook/"]
COPY ["XYPortal.PasswordBook/common.props", "XYPortal.PasswordBook/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.Application/XYPortal.PasswordBook.Application.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.Application/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.Application.Contracts/XYPortal.PasswordBook.Application.Contracts.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.Application.Contracts/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.Domain/XYPortal.PasswordBook.Domain.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.Domain/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.Domain.Shared/XYPortal.PasswordBook.Domain.Shared.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.Domain.Shared/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.EntityFrameworkCore/XYPortal.PasswordBook.EntityFrameworkCore.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.EntityFrameworkCore/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.HttpApi/XYPortal.PasswordBook.HttpApi.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.HttpApi/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.HttpApi.Client/XYPortal.PasswordBook.HttpApi.Client.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.HttpApi.Client/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.Installer/XYPortal.PasswordBook.Installer.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.Installer/"]
COPY ["XYPortal.PasswordBook/src/XYPortal.PasswordBook.Web/XYPortal.PasswordBook.Web.csproj", "XYPortal.PasswordBook/src/XYPortal.PasswordBook.Web/"]
COPY ["XYPortal.PasswordBook/test/XYPortal.PasswordBook.Application.Tests/XYPortal.PasswordBook.Application.Tests.csproj", "XYPortal.PasswordBook/test/XYPortal.PasswordBook.Application.Tests/"]
COPY ["XYPortal.PasswordBook/test/XYPortal.PasswordBook.Domain.Tests/XYPortal.PasswordBook.Domain.Tests.csproj", "XYPortal.PasswordBook/test/XYPortal.PasswordBook.Domain.Tests/"]
COPY ["XYPortal.PasswordBook/test/XYPortal.PasswordBook.EntityFrameworkCore.Tests/XYPortal.PasswordBook.EntityFrameworkCore.Tests.csproj", "XYPortal.PasswordBook/test/XYPortal.PasswordBook.EntityFrameworkCore.Tests/"]
COPY ["XYPortal.PasswordBook/test/XYPortal.PasswordBook.TestBase/XYPortal.PasswordBook.TestBase.csproj", "XYPortal.PasswordBook/test/XYPortal.PasswordBook.TestBase/"]

RUN cd XYPortal.LinkBoard && \
    dotnet restore && \
    dotnet build -c Release

RUN cd ../XYPortal.RandomStringProvider && \
    dotnet restore && \
    dotnet build -c Release

RUN cd ../XYPortal.PasswordBook && \
    dotnet restore && \
    dotnet build -c Release

RUN cd ../XYPortal && \
    dotnet restore

FROM restore AS build
# 下面两部很关键否则dotnet build时会因为找不到Program.cs而编译失败
COPY . .

WORKDIR /XYPortal/src/XYPortal.HttpApi.Host

RUN dotnet build XYPortal.HttpApi.Host.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish XYPortal.HttpApi.Host.csproj -c Release -o /app/publish

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "XYPortal.HttpApi.Host.dll" ]