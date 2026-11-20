## 滴答清单 Open API 认证

1. 登录[滴答清单网页版](https://dida365.com/webapp/)。
2. 进入 **头像 → 设置 → 账户与安全 → API 口令**，创建并复制口令。
3. GitHub Actions 的仓库 Secret 使用 `DIDA_API`，值为口令原文。本地 CLI 将口令填入 `token.json` 的 `DidaApiToken`，Windows 图形界面从环境变量 `DIDA_API` 读取。

从旧版本升级时，原来的 `DIDA_CREDENTIAL`、手机号密码和网页 Cookie 配置需要换成 API 口令。请将口令保存在 Secret 或本地配置文件里，避免把它提交到仓库。

同步通过滴答清单的 [Open API](https://developer.dida365.com/) 读取清单、未完成和已完成的任务。新建任务会附上一条独立的来源评论，内容为 Canvas 链接。全局通知的来源评论保存通知 ID。旧任务评论和之前写入正文的 `TodoSync: …` 标记仍可用于识别已有任务。

同步会保存加密的任务来源索引。首次识别旧任务时会读取评论；之后会先核对滴答任务仍在清单里，再复用已有映射。GitHub Actions 通过缓存保留索引，本地运行将索引写入当前用户的应用数据目录。首次运行、更换 API 口令或缓存失效时，需要重新读取未标记的旧任务评论。

若返回 HTTP 401 或 403，请检查口令是否有效并确认对应清单允许当前账号写入。
