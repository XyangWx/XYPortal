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

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

RUN apt-get clean && apt-get update && \
    apt-get install -y --no-install-recommends \
        locales \
        tzdata \
        curl \
    && \
    sed -i '/zh_CN.UTF-8/s/^# //g' /etc/locale.gen || echo 'zh_CN.UTF-8 UTF-8' >> /etc/locale.gen && \
    locale-gen zh_CN.UTF-8 && \
    ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone && \
    rm -rf /var/lib/apt/lists/*

ENV LANG=zh_CN.UTF-8
ENV LANGUAGE=zh_CN:zh
ENV LC_ALL=zh_CN.UTF-8

RUN update-locale LANG=zh_CN.UTF-8 LC_ALL=zh_CN.UTF-8

RUN dotnet tool install -g Volo.Abp.Studio.Cli && \
    curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.4/install.sh | bash && \
    NVM_DIR=$HOME/.nvm && \
    [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh" && \
    PATH=$PATH:$HOME/.dotnet/tools && \
    nvm install v25.9.0

