#!/bin/bash
#
# Vue 项目构建脚本
# 用法:
#   ./build.sh                                    # 使用 .env 中的现有值
#   ./build.sh -a "http://api.example.com"        # 只更新 VITE_API_BASE_URL
#   ./build.sh -o "http://auth.example.com"       # 只更新 VITE_AUTH_SERVER_URL
#   ./build.sh -a "http://api.example.com" -o "http://auth.example.com"  # 同时更新
#

set -euo pipefail

A_VALUE=""
O_VALUE=""

# ---------- 解析参数 ----------
while getopts "a:o:" opt; do
    case "$opt" in
        a) A_VALUE="$OPTARG" ;;
        o) O_VALUE="$OPTARG" ;;
        *) echo "用法: $0 [-a VITE_API_BASE_URL] [-o VITE_AUTH_SERVER_URL]"
           exit 1 ;;
    esac
done

PROJECT_ROOT="$(cd "$(dirname "$0")" && pwd)"
ENV_FILE="$PROJECT_ROOT/.env"

echo "========================================"
echo "  Vue 项目构建脚本"
echo "========================================"
echo ""

# ---------- 检查 .env 文件 ----------
if [[ ! -f "$ENV_FILE" ]]; then
    echo "错误: .env 文件不存在: $ENV_FILE" >&2
    exit 1
fi

# ---------- 先备份原始 .env ----------
cp "$ENV_FILE" "$ENV_FILE.bak"

# ---------- 更新 .env ----------
update_env_var() {
    local var_name="$1"
    local var_value="$2"
    local file="$3"

    if [[ -z "$var_value" ]]; then
        echo "  [跳过] $var_name 未提供新值，保留现有配置"
        return
    fi

    if grep -q "^${var_name}=" "$file"; then
        sed -i "s|^${var_name}=.*|${var_name}=${var_value}|" "$file"
        echo "  [更新] ${var_name}=${var_value}"
    else
        echo "" >> "$file"
        echo "${var_name}=${var_value}" >> "$file"
        echo "  [新增] ${var_name}=${var_value}"
    fi
}

if [[ -n "$A_VALUE" ]]; then
    echo "更新 VITE_API_BASE_URL:"
    update_env_var "VITE_API_BASE_URL" "$A_VALUE" "$ENV_FILE"
fi

if [[ -n "$O_VALUE" ]]; then
    echo "更新 VITE_AUTH_SERVER_URL:"
    update_env_var "VITE_AUTH_SERVER_URL" "$O_VALUE" "$ENV_FILE"
fi

if [[ -z "$A_VALUE" && -z "$O_VALUE" ]]; then
    echo "未提供参数，使用 .env 中现有配置进行构建"
fi

echo ""

# ---------- 执行构建 ----------
echo "开始构建 Vue 项目..."

cd "$PROJECT_ROOT"

# 检查 node_modules 是否存在，不存在则先安装依赖
if [[ ! -d "node_modules" ]]; then
    echo "node_modules 不存在，正在安装依赖..."
    npm install
fi

cleanup() {
    # 恢复原始 .env
    cp "$ENV_FILE.bak" "$ENV_FILE"
    rm -f "$ENV_FILE.bak"
}
trap cleanup EXIT

echo "执行 npm run build ..."
npm run build

echo ""
echo "========================================"
echo "  构建成功!"
echo "========================================"
