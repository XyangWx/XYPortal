# XYPortal.AuthServer Dockerfile
#
# 构建上下文: /data/repositories/xyportal/ （XYPortal 父目录）
# 构建命令:
#   docker buildx build -f XYPortal/src/XYPortal.AuthServer/Dockerfile .. -t auth_server:1.0.1
#
# 说明:
# - XYPortal.slnx 位于 XYPortal/ 目录下
# - XYPortal.LinkBoard、XYPortal.PasswordBook 源码位于 XYPortal/ 同级目录
# - 因此构建上下文须设为父目录，使所有模块源码均可被 COPY

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
# 构建上下文为 /data/repositories/xyportal/
# XYPortal/ 解决方案文件及主项目在 XYPortal/ 子目录下
# LinkBoard / PasswordBook 等模块源码在 XYPortal/ 同级目录下
#
# 重要: XYPortal.Domain.Shared.csproj 中引用路径为 ../../../XYPortal.LinkBoard/src/...
#       即从 /XYPortal/src/XYPortal.Domain.Shared/ 上跳3层到 /XYPortal/
#       再进入 XYPortal.LinkBoard/src/，对应容器中 /XYPortal/XYPortal.LinkBoard/src/
# ============================================================

# 复制 LinkBoard 模块源码（放到 /XYPortal/XYPortal.LinkBoard/src/）
COPY XYPortal.LinkBoard/src/ /XYPortal/XYPortal.LinkBoard/src/
COPY XYPortal.LinkBoard/test/ /XYPortal/XYPortal.LinkBoard/test/

# 复制 PasswordBook 模块源码（放到 /XYPortal/XYPortal.PasswordBook/src/）
COPY XYPortal.PasswordBook/src/ /XYPortal/XYPortal.PasswordBook/src/
COPY XYPortal.PasswordBook/test/ /XYPortal/XYPortal.PasswordBook/test/

WORKDIR /XYPortal

# 复制解决方案文件
COPY XYPortal/XYPortal.slnx ./
COPY XYPortal/common.props ./

# 复制主项目源码（XYPortal 下的 src 和 test）
COPY XYPortal/src/  ./src/
COPY XYPortal/test/ ./test/

# 还原 NuGet 包（还原整个 slnx）
RUN dotnet restore

# ============================================================
# 安装前端依赖并运行 abp install-libs
# ============================================================
WORKDIR /XYPortal/src/XYPortal.AuthServer
RUN yarn install --frozen-lockfile || yarn install
RUN dotnet run --project . -- abp install-libs || true

# ============================================================
# 回到 XYPortal 根目录执行发布
# 关键: 先清除 obj 缓存，确保 publish 时 dotnet 重新完整解析
#       LinkBoard/PasswordBook 的项目引用（而非使用已被缓存的"未找到"结果）
# ============================================================
WORKDIR /XYPortal

# 清除 Domain.Shared 和 Domain 的 restore 缓存，
# 让 publish 重新完整解析 LinkBoard/PasswordBook 传递依赖
RUN find src/XYPortal.Domain.Shared/obj -type f -delete 2>/dev/null || true
RUN find src/XYPortal.Domain/obj -type f -delete 2>/dev/null || true

RUN dotnet publish src/XYPortal.AuthServer/XYPortal.AuthServer.csproj \
        -c Release \
        -o /app/publish

# ============================================
# Stage 3: final —运行时镜像
# ============================================
FROM base AS final
WORKDIR /app

# 复制发布产物（包含 Views、wwwroot、Pages 等）
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "XYPortal.AuthServer.dll"]
