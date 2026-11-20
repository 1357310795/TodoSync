# 滴答清单认证与 GitHub Actions

如果密码登录返回 `need_captcha`，表示服务端要求验证码。自动任务无法完成交互验证，可以使用自己已经登录的网页会话。

## 使用已有会话（推荐用于定时任务）

1. 在浏览器打开 https://dida365.com/webapp/ 并正常登录，按网页要求完成验证码。
2. 打开开发者工具，进入 **Application（应用）→ Cookies → dida365.com**。找到名称为 `t` 的 Cookie，只复制其 Value，不要复制 `t=` 或其他 Cookie。
3. 在本仓库 **Settings → Secrets and variables → Actions** 中编辑 `DIDA_CREDENTIAL`，将其完整内容替换为：

```json
{"token":"在这里填写 Cookie t 的值"}
```

4. 保存 Secret 后运行“同步到滴答清单”。合并代码前验证时，选择包含该修改的 PR 分支。

`token` 是网页登录会话，不是 OAuth access token，也不是验证码。它等同于登录凭据，只放在 GitHub Secret 中，不要发到聊天、Issue、PR、日志或提交到仓库。

程序会携带该会话调用只读同步接口，验证通过后才继续同步。会话无效、过期或网络异常时会停止，不会自动尝试密码登录；需要更新会话时重复以上步骤。

## 原有手机号密码方式

仍可使用原来的配置，无需把 phone 改成 username：

```json
{"phone":"你的手机号","password":"你的密码"}
```

此方式在每次同步时登录，可能遇到验证码。若同时配置 token，会优先使用 token；空 token 会报错，不会回退到密码。

## 诊断

- `need_upgrade_client`：服务端要求更新客户端版本信息。代码中的 X-Device 版本使用 2026-10-28 官网网页版本 8240，以后若再次出现该错误需重新核对官网。
- `need_captcha`：要求交互验证码；使用上述有效网页会话。
- `username_password_not_match`：账号认证未通过，需要结合账号字段与凭据检查，不能仅凭 HTTP 500 判断密码是否错误。
- 会话验证 HTTP 401/403：检查会话是否失效；5xx 或网络异常不代表密码错误。

PR 触发的工作流只编译，不读取账号 Secret、不执行同步；手动和定时触发才执行实际同步。
