## 滴答清单认证

### 获取网页会话凭据

1. 打开[滴答清单网页版](https://dida365.com/webapp/)，登录并按网页提示完成验证。
2. 打开浏览器开发者工具，进入 **Application（应用）→ Cookies → dida365.com**。
3. 找到名称为 `t` 的 Cookie，仅复制 **Value** 列的内容。配置中的 `token` 填这个值，省略 `t=` 前缀。

网页会话凭据具备账号访问权限。请保存在 GitHub Actions Secret 或本地 `token.json` 中，避免将凭据写入日志或提交到仓库。

### 选择部署方式

- [GitHub Actions 部署指南](./actions-persis.md#三配置滴答清单登录信息若要同步到滴答清单)中的 `DIDA_CREDENTIAL` 使用 `{"token":"Cookie t 的值"}`。
- [本地部署指南](./local.md#三更新本地-token-存储文件)中的 `token.json` 将同一对象填在 `DidaCredential` 字段下。

原有的 `phone` 和 `password` 配置仍可使用。配置了非 `null` 的 `token` 时，程序优先验证网页会话。会话校验失败时，本次运行会停止。

### 常见错误

- `need_captcha` 表示密码登录需要在网页上完成验证码。完成验证后，可改用网页会话凭据。
- `username_password_not_match` 表示密码登录未通过，请检查 `phone` 和 `password`。
- `need_upgrade_client` 表示服务端要求更新客户端版本信息。
- 会话验证返回 HTTP 401 或 403 时，重新登录网页版并更新 Cookie `t`。如日志报告无法解析同步响应，可提供 HTTP 状态码或日志里的字段名协助排查。
