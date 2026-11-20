## 滴答清单 Open API 认证

1. 登录[滴答清单网页版](https://dida365.com/webapp/)。
2. 进入 **头像 → 设置 → 账户与安全 → API 口令**，创建并复制口令。
3. GitHub Actions 的仓库 Secret 使用 `DIDA_API`，值为口令原文。本地 CLI 将口令填入 `token.json` 的 `DidaApiToken`，Windows 图形界面从环境变量 `DIDA_API` 读取。

从旧版本升级时，原来的 `DIDA_CREDENTIAL`、手机号密码和网页 Cookie 配置需要换成 API 口令。请将口令保存在 Secret 或本地配置文件里，避免把它提交到仓库。

同步通过滴答清单的 [Open API](https://developer.dida365.com/) 读取清单、未完成和已完成的任务。旧任务继续通过评论里的 Canvas 链接或通知 ID 匹配。新建或更新的任务会在正文或检查清单说明末尾保存一行 `TodoSync: …`，供下次同步识别，请保留这一行。

若返回 HTTP 401 或 403，请检查口令是否有效并确认对应清单允许当前账号写入。
