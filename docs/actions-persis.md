## 基于 GitHub Actions 的 Canvas-Todo 同步
### 特性
- 基于 GitHub Actions，每两小时运行一次同步
- Graph Token 持久化，无需手动更新

### 配置步骤（以 Windows 系统为例）
1. fork [这个仓库](https://github.com/1357310795/TodoSynchronizer)，注意**取消勾选**“Copy the master branch only”

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

10. 访问[这个链接](https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id=49694ef2-8751-4ac9-8431-8817c27350b4&response_type=code&redirect_uri=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fnativeclient&response_mode=query&scope=Tasks.ReadWrite%20User.Read%20offline_access&state=12345)，登录 Microsoft 账户

![](https://s2.loli.net/2022/08/21/B7Kj6a5tJPXQqSL.png)

11. 授权 MyTodoApp（务必核对权限是否与图中一致）

![](https://s2.loli.net/2022/08/21/JiYnCMUPshc5RGd.png)

12. 完成后，在地址栏找到 code 参数（**看清楚**不要把&之类的字符也算进去了）

![](https://s2.loli.net/2022/08/21/nD6NeU3PkhaAs5H.png)

13. 按 Win+R，输入 cmd，打开终端（不建议使用 Windows Terminal 和 Powershell）

![](https://s2.loli.net/2022/08/21/3DiI6pwYgJFzufk.png)

14. 粘贴以下命令并运行

```bash
curl -d "client_id=49694ef2-8751-4ac9-8431-8817c27350b4&scope=Tasks.ReadWrite%20User.Read%20offline_access&redirect_uri=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fnativeclient&grant_type=authorization_code&code=【这里换成你的code！】" https://login.microsoftonline.com/consumers/oauth2/v2.0/token
```

15. 在一长串字符中找到“refresh_token”，鼠标拖动选中**冒号后面引号里面**的一长串字符，右键点击即可复制

![](https://s2.loli.net/2022/08/21/LNzU9G5k7eowJRS.png)

16. 将 refresh_token 保存到文本文件

![](https://s2.loli.net/2022/08/21/W6JOvjIprDw81iU.png)

17. 打开 Git Bash，使用下面的命令加密这个文本文件（注意使用强密码！丢失也没关系，再按步骤做一遍就行！）

```bash
gpg --passphrase "【你的强密码】" --batch -o "graphtoken.asc"  -c --armor "【输入文件】"
```
![](https://s2.loli.net/2022/08/22/d8KYZ1cqRkMJlFi.png)

18. 打开输出文件，复制内容

![](https://s2.loli.net/2022/08/21/QKtaHz6xNI5lbZJ.png)

19. 在自己 fork 的仓库，切换到 graphtoken 分支

![](https://s2.loli.net/2022/08/21/NzJRe4E5LSlYVGb.png)

20. 编辑 graphtoken.asc 文件

![](https://s2.loli.net/2022/08/21/Rx4rTsCJ8L2hejA.png)

21. 粘贴刚才复制的内容，直接提交

![](https://s2.loli.net/2022/08/21/fdGqptNy4FZc9Vz.png)

22. 再次到“Secrets”里，创建一个新条目，“Name”为“PASSWORD”，“Value”为刚才加密用的密码

![](https://s2.loli.net/2022/08/21/iupSOXaRbE3Fjxo.png)

23. 打开“Actions”选项卡，点击按钮启用

![](https://s2.loli.net/2022/08/21/qtCnKdpPWRFNbgM.png)

24. 左侧选择“TodoSync”，右侧点击“Run workflow”

![](https://s2.loli.net/2022/08/21/2kcXUByTOaoLIiv.png)

25. 刷新，可进入 Action 查看详情。Run 步骤下面的内容为程序的执行输出

![](https://s2.loli.net/2022/08/21/kmUFi2YlMH1xbuK.png)

26. 编辑仓库根目录下的 `config.yaml` 文件，可以调整程序设置

![](https://s2.loli.net/2022/08/22/mcK5afDhRXSUCVM.png)

### Q&A
#### Fork 的仓库能不能设置为 Private？

对于公开仓库，GitHub Actions 是免费的；而对于私有仓库，GitHub Actions 每个月有 2000 分钟的免费额度，超出会有巨额收费（在[这里](https://github.com/settings/billing)看用量）。本项目每次运行大约需要 4min，也就是说每个月顶多能运行 500 次，属于刚好够用。但是**强烈建议保持仓库为公开状态**。

#### 为什么步骤这么复杂？我的账号、Token 安全吗？

复杂的配置步骤就是为了保证账号和令牌的安全性。Canvas Token 的安全性由 GitHub 保证，Graph Token 的安全性由 AES 算法和您在第 17 步使用的密码保证。

#### 为什么不直接把 Graph 的 AccessToken/RefreshToken 保存到 Secrets，像 Canvas Token 那样？

AccessToken 的有效期只有 1 小时，RefreshToken 的有效期可能是 90 天（参考[这里](https://docs.microsoft.com/zh-cn/azure/active-directory/develop/active-directory-configurable-token-lifetimes#refresh-and-session-token-lifetime-policy-properties)，我看不明白）。为确保令牌永不过期，需要在每次运行时更新令牌。Secrets 不支持使用 Action 操作更新，故只能将令牌加密后保存到存储库的 graphtoken 分支，密钥保存在 Secrets 内。

#### 授权 MyTodoApp 有什么风险？如何取消此授权？

正如授权时显示的应用权限所示，“创建、读取、更新和删除你的任务和计划”和“保持对已向 MyTodoApp 授予访问权限的数据的访问权限”是应用正常运行必须的权限，“读取您的个人资料”用于验证登录。程序不会也不可能索要您的敏感信息。在[这里](https://account.live.com/consent/Manage)可以管理连接到 Microsoft 账户的应用。

#### 无法登录微软账号？

换用个人邮箱注册的账号，不要用学校/机构的账号。

#### curl 命令不可用？

请升级您的古老的 Windows 7/XP 系统，或者使用 Postman 代替

#### 找不到 refresh_token？

请再次检查 curl 命令是否输入正确

#### 不知道什么是 Git Bash？没安装 Git？找不到 gpg 命令？

去下载一个 [gpg4win](https://www.gpg4win.org/) 也行。

#### GitHub Action 运行失败？

如果不明白为什么失败，到原仓库提交 issue