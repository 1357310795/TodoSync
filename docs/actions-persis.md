## 配置步骤
### 一、配置 Canvas Token

1. 点击右上角的 “fork” 复制这个仓库，注意**取消勾选**“Copy the master branch only”（若没注册过 GitHub 账号，按照指示注册一个）

![](https://s2.loli.net/2022/08/21/STBVjWhLsU6Dib1.png)

2. 打开 Canvas，进入“设置”页面

![](https://s2.loli.net/2022/08/21/bdnaM9jLhvCI4i3.png)

3. 点击“创建新访问许可证”

![](https://s2.loli.net/2022/08/21/FheNU1Rlz7X5cgS.png)

4. “用途”随便填一个，“过期”建议留空，点击“生成令牌”

![](https://s2.loli.net/2022/08/21/riymJ4DqvI2ZAPb.png)

5. 复制生成的令牌

![](https://s2.loli.net/2022/08/21/Eyej95vY3cCsVZT.png)

6. 在**自己 fork 的仓库**，进入“Settings”

![](https://s2.loli.net/2022/08/21/BuWYEbml4QsVUXq.png)

7. 选择“Secrets - Actions”，点击“New repository secret”

![](https://s2.loli.net/2022/08/21/FavMKjp4lGYIh6g.png)

8. “Name”为“CANVAS_TOKEN”，“Value”为刚才复制的令牌

![](https://s2.loli.net/2022/08/21/lNoKxvDZgFXWkyA.png)

9. 添加 Canvas Token 完成

![](https://s2.loli.net/2022/08/21/kULpJbrxvgEGzCQ.png)

### 二、配置 Graph Token

下面以 Windows 系统为例，借助“Graph 认证辅助工具”（在本仓库开源）进行配置。如有其它需求，可参考[手动配置 Graph Token](./graph-token-manually.md)

10.  在 [Releases](../../../releases) 页面下载“TodoSynchronizer.QuickTool.exe”，打开。

11.  点击“获取 Graph 认证”，跳转到浏览器

12.  授权 MyTodoApp（务必核对权限是否与图中一致）

![](https://s2.loli.net/2022/08/21/JiYnCMUPshc5RGd.png)

13. 完成后，回到程序，输入密钥以保护用户凭证

![](https://s2.loli.net/2022/09/15/FH8TxICgpovdhs7.png)
![](https://s2.loli.net/2022/09/14/Em9aWxLCMkZf23H.png)

1.  在自己 fork 的仓库，切换到 graphtoken 分支

![](https://s2.loli.net/2022/08/21/NzJRe4E5LSlYVGb.png)

15. 编辑 graphtoken.asc 文件

![](https://s2.loli.net/2022/09/14/6DLgz7mHQdVSnZ2.png)

16. 粘贴刚才复制的内容（把原先的**覆盖**掉，注意不要有多余的换行），直接提交

![](https://s2.loli.net/2022/08/21/fdGqptNy4FZc9Vz.png)

17. 再次到“Secrets”里，创建一个新条目，“Name”为“KEY”，“Value”为刚才输入的密钥

![](https://s2.loli.net/2022/09/14/4akGQOzVLH7nYvC.png)

### 三、启动定时任务

18. 点击“Actions”选项卡，点击按钮启用

![](https://s2.loli.net/2022/08/21/qtCnKdpPWRFNbgM.png)

19. 左侧选择“TodoSync”，右侧点击“Run workflow”

![](https://s2.loli.net/2022/08/21/2kcXUByTOaoLIiv.png)

20. 刷新，可进入 Action 查看详情。Run 步骤下面的内容为程序的执行输出

![](https://s2.loli.net/2022/08/21/kmUFi2YlMH1xbuK.png)

21. 编辑仓库根目录下的 `config.yaml` 文件，可以调整同步程序设置

![](https://s2.loli.net/2022/08/22/mcK5afDhRXSUCVM.png)

### Q&A
#### Fork 的仓库能不能设置为 Private？

对于公开仓库，GitHub Actions 是免费的；而对于私有仓库，GitHub Actions 每个月有 2000 分钟的免费额度，超出会有巨额收费（在[这里](https://github.com/settings/billing)看用量）。本项目每次运行大约需要 2min，也就是说每个月顶多能运行 1000 次，可能不够用。**强烈建议保持仓库为公开状态**。

#### 为什么步骤这么复杂？我的账号、Token 安全吗？

复杂的配置步骤就是为了保证账号和令牌的安全性。Canvas Token 的安全性由 GitHub 保证，Graph Token 的安全性由 AES 算法保证。

#### 为什么不直接把 Graph 的 AccessToken/RefreshToken 保存到 Secrets，像 Canvas Token 那样？

AccessToken 的有效期只有 1 小时，RefreshToken 的有效期可能是 90 天（参考[这里](https://docs.microsoft.com/zh-cn/azure/active-directory/develop/active-directory-configurable-token-lifetimes#refresh-and-session-token-lifetime-policy-properties)，我看不明白）。为确保令牌永不过期，需要在每次运行时更新令牌。Secrets 不支持使用 Action 操作更新，故只能将令牌加密后保存到存储库的 graphtoken 分支，密钥保存在 Secrets 内。

#### 授权 MyTodoApp 有什么风险？如何取消此授权？

正如授权时显示的应用权限所示，“创建、读取、更新和删除你的任务和计划”和“保持对已向 MyTodoApp 授予访问权限的数据的访问权限”是应用正常运行必须的权限，“读取您的个人资料”用于验证登录。程序不会也不可能索要您的敏感信息。在[这里](https://account.live.com/consent/Manage)可以管理连接到 Microsoft 账户的应用。

#### 无法登录微软账号？该 Microsoft 帐户不存在？

换用个人邮箱注册的账号，不要用学校/机构的账号。

#### GitHub Action 运行失败？

如果不明白为什么失败，到原仓库提交 issue

#### 如果仓库设置为 Public，如何隐藏我的课程信息？

编辑`config.yaml`文件，把`VerboseMode`改成`false`