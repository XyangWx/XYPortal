#!/bin/sh
# DbMigrator 配置与执行脚本
# 功能：交互式配置 appsettings.json，然后执行 DbMigrator
# 兼容：POSIX sh (sh/dash/ash)

set -e

# ============================================================
# 变量定义
# ============================================================
CONFIG_FILE="appsettings.json"
BACKUP_FILE="appsettings.json.bak"

# 默认值（数据库连接）
DEFAULT_HOST="192.168.1.55"
DEFAULT_PORT="5432"
DEFAULT_DATABASE="abp_xyportal"
DEFAULT_USER="abpadmin"
DEFAULT_PASSWORD="NewUser@123"

# ============================================================
# 函数定义
# ============================================================

# 恢复配置文件（trap 调用）
restore_config() {
    echo ""
    echo "恢复原配置文件..."
    if [ -f "$BACKUP_FILE" ]; then
        cp "$BACKUP_FILE" "$CONFIG_FILE"
        rm "$BACKUP_FILE"
        echo "已恢复"
    fi
}

# 检查依赖工具
check_dependencies() {
    echo "检查依赖工具..."

    # 检查 sed
    if ! command -v sed >/dev/null 2>&1; then
        echo "错误: sed 不可用"
        echo "提示: 请使用包含 sed 的环境，或使用完整镜像(dotnet/sdk)"
        exit 1
    fi

    # 检查 dotnet
    if ! command -v dotnet >/dev/null 2>&1; then
        echo "错误: dotnet 不可用"
        exit 1
    fi

    echo "依赖检查通过"
}

# 读取当前连接字符串中的值
get_current_value() {
    field="$1"
    value=""

    case "$field" in
        Host)
            value=$(sed -n 's/.*Host=\([^;]*\);.*/\1/p' "$CONFIG_FILE" | tr -d '"')
            ;;
        Port)
            value=$(sed -n 's/.*Port=\([^;]*\);.*/\1/p' "$CONFIG_FILE" | tr -d '"')
            ;;
        Database)
            value=$(sed -n 's/.*Database=\([^;]*\);.*/\1/p' "$CONFIG_FILE" | tr -d '"')
            ;;
        User)
            value=$(sed -n 's/.*User ID=\([^;]*\);.*/\1/p' "$CONFIG_FILE" | tr -d '"')
            ;;
        Password)
            value=$(sed -n 's/.*Password=\([^;]*\);.*/\1/p' "$CONFIG_FILE" | tr -d '"')
            ;;
    esac

    echo "${value:-}"
}

# 显示当前配置
show_current_config() {
    echo ""
    echo "========================================"
    echo "  当前配置文件内容"
    echo "========================================"
    echo ""
    cat "$CONFIG_FILE"
    echo ""
}

# 询问数据库配置
prompt_database_config() {
    echo ""
    echo "========================================"
    echo "  数据库配置（直接回车使用默认值）"
    echo "========================================"

    # Host
    printf "Host (默认: %s): " "$DEFAULT_HOST"
    IFS= read -r input
    NEW_HOST="${input:-$DEFAULT_HOST}"

    # Port
    printf "Port (默认: %s): " "$DEFAULT_PORT"
    IFS= read -r input
    NEW_PORT="${input:-$DEFAULT_PORT}"

    # Database
    printf "Database (默认: %s): " "$DEFAULT_DATABASE"
    IFS= read -r input
    NEW_DATABASE="${input:-$DEFAULT_DATABASE}"

    # User
    printf "User ID (默认: %s): " "$DEFAULT_USER"
    IFS= read -r input
    NEW_USER="${input:-$DEFAULT_USER}"

    # Password
    printf "Password (默认: %s): " "$DEFAULT_PASSWORD"
    IFS= read -r input
    NEW_PASSWORD="${input:-$DEFAULT_PASSWORD}"
}

# 使用 sed 更新数据库配置
apply_database_config() {
    # 注意: 连接字符串格式为 "Host=xxx;Port=xxx;..." 需要逐项替换
    # 使用 "Host=[^;]*;" 匹配 Host=值; 的模式
    sed -i "s/Host=[^;]*/Host=$NEW_HOST/" "$CONFIG_FILE"
    sed -i "s/Port=[^;]*/Port=$NEW_PORT/" "$CONFIG_FILE"
    sed -i "s/Database=[^;]*/Database=$NEW_DATABASE/" "$CONFIG_FILE"
    sed -i "s/User ID=[^;]*/User ID=$NEW_USER/" "$CONFIG_FILE"
    sed -i "s/Password=[^;]*/Password=$NEW_PASSWORD/" "$CONFIG_FILE"
}

# 询问 OpenIddict 应用配置
prompt_openiddict_apps() {
    echo ""
    echo "========================================"
    echo "  OpenIddict 应用配置"
    echo "========================================"
    echo ""

    # 清空现有 OpenIddict 部分并准备追加
    # 先找到 OpenIddict 开始的行号
    start_line=$(grep -n '"OpenIddict"' "$CONFIG_FILE" | cut -d: -f1)
    if [ -n "$start_line" ]; then
        # 删除从 OpenIddict 开始到文件末尾的行
        sed -i ''"$start_line"',$d' "$CONFIG_FILE"
        # 确保最后一行（ConnectionStrings 闭合行）末尾有逗号
        last_line=$((start_line - 1))
        sed -i "${last_line}s/[^,]$/&,/" "$CONFIG_FILE"
    fi

    APPS_JSON=""
    APP_COUNT=0

    # 添加第一个应用（必须）
    echo "--- 应用 $((APP_COUNT + 1)) ---"
    printf "ClientId: "
    IFS= read -r CLIENT_ID
    if [ -z "$CLIENT_ID" ]; then
        echo "错误: 至少需要配置 1 个应用"
        return 1
    fi

    printf "ClientSecret (可空): "
    IFS= read -r CLIENT_SECRET
    printf "RootUrl: "
    IFS= read -r ROOT_URL

    # 拼接第一个应用
    APP_ENTRY="\"$CLIENT_ID\": {
          \"ClientId\": \"$CLIENT_ID\","
    [ -n "$CLIENT_SECRET" ] && APP_ENTRY="$APP_ENTRY
          \"ClientSecret\": \"$CLIENT_SECRET\","
    APP_ENTRY="$APP_ENTRY
          \"RootUrl\": \"$ROOT_URL\"
        }"
    APPS_JSON="$APP_ENTRY"
    APP_COUNT=$((APP_COUNT + 1))

    # 循环询问是否添加更多应用
    while true; do
        echo ""
        printf "是否添加新应用? (y/n): "
        IFS= read -r add_more
        case "$add_more" in
            y|Y)
                echo ""
                echo "--- 应用 $((APP_COUNT + 1)) ---"
                printf "ClientId: "
                IFS= read -r CLIENT_ID
                [ -z "$CLIENT_ID" ] && break

                printf "ClientSecret (可空): "
                IFS= read -r CLIENT_SECRET
                printf "RootUrl: "
                IFS= read -r ROOT_URL

                # 拼接后续应用
                APP_ENTRY="\"$CLIENT_ID\": {
          \"ClientId\": \"$CLIENT_ID\","
                [ -n "$CLIENT_SECRET" ] && APP_ENTRY="$APP_ENTRY
          \"ClientSecret\": \"$CLIENT_SECRET\","
                APP_ENTRY="$APP_ENTRY
          \"RootUrl\": \"$ROOT_URL\"
        }"
                APPS_JSON="$APPS_JSON,
        $APP_ENTRY"
                APP_COUNT=$((APP_COUNT + 1))
                ;;
            *)
                break
                ;;
        esac
    done

    # 构建完整的 OpenIddict JSON
    OPENIDDICT_JSON="
  \"OpenIddict\": {
    \"Applications\": {
      $APPS_JSON
    }
  }
}"

    # 在文件末尾添加 OpenIddict 配置
    # 先确保最后有换行
    echo "" >> "$CONFIG_FILE"
    printf '%s\n' "$OPENIDDICT_JSON" >> "$CONFIG_FILE"

    echo ""
    echo "已添加 $APP_COUNT 个应用"
}

# 显示新配置并确认
confirm_config() {
    echo ""
    echo "========================================"
    echo "  修改后的配置"
    echo "========================================"
    echo ""
    cat "$CONFIG_FILE"
    echo ""
    echo "========================================"
    printf "是否执行 DbMigrator? (y/n): "
    IFS= read -r confirm
    case "$confirm" in
        y|Y) return 0 ;;
        *)   return 1 ;;
    esac
}

# ============================================================
# 主流程
# ============================================================

echo "========================================"
echo "  DbMigrator 配置脚本"
echo "========================================"

# 步骤 1: 检查依赖
check_dependencies

# 步骤 2: 备份原配置文件
echo ""
echo "备份原配置文件..."
cp "$CONFIG_FILE" "$BACKUP_FILE"

# 步骤 3: 显示当前配置
show_current_config

# 步骤 4: 询问数据库配置
prompt_database_config

# 步骤 5: 询问 OpenIddict 应用配置
if ! prompt_openiddict_apps; then
    echo "配置失败，恢复原文件..."
    restore_config
    exit 1
fi

# 步骤 6: 应用数据库配置修改
apply_database_config

# 步骤 7: 确认修改
if ! confirm_config; then
    echo "用户取消操作"
    restore_config
    exit 0
fi

# 步骤 8: 注册 trap 钩子（确保退出时恢复）
trap 'restore_config' EXIT

# 步骤 9: 执行 DbMigrator
echo ""
echo "========================================"
echo "  执行 DbMigrator..."
echo "========================================"
./XYPortal.DbMigrator

# 步骤 10 & 11: 恢复原配置文件并清理
trap - EXIT
cp "$BACKUP_FILE" "$CONFIG_FILE"
rm "$BACKUP_FILE"

echo ""
echo "========================================"
echo "  执行完成，配置已恢复"
echo "========================================"
