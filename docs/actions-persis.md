## 基于 GitHub Actions 的 Canvas-Todo 同步
### 特性
- 基于 GitHub Actions，每两小时运行一次同步
- Graph Token 持久化，无需手动更新

### 配置步骤
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

10. 访问[这个链接](https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id=49694ef2-8751-4ac9-8431-8817c27350b4&response_type=code&redirect_uri=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fnativeclient&response_mode=query&scope=Tasks.ReadWrite%20offline_access&state=12345)，登录 Microsoft 账户
![](https://s2.loli.net/2022/08/21/B7Kj6a5tJPXQqSL.png)

11. 授权 MyTodoApp
![](https://s2.loli.net/2022/08/21/ukbdAHSGTvCsZpP.png)

12. 完成后，在地址栏找到 code 参数（**看清楚**不要把&之类的字符也算进去了）
![](https://s2.loli.net/2022/08/21/nD6NeU3PkhaAs5H.png)

13. 按 Win+R，输入 cmd，打开终端（不建议使用 Windows Terminal 和 Powershell）
![](https://s2.loli.net/2022/08/21/3DiI6pwYgJFzufk.png)

14. 粘贴以下命令并运行
```bash
curl -d "client_id=49694ef2-8751-4ac9-8431-8817c27350b4&scope=Tasks.ReadWrite%20offline_access&code=【这里换成你的code！】&redirect_uri=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fnativeclient&grant_type=authorization_code" https://login.microsoftonline.com/consumers/oauth2/v2.0/token
```

15. 在一长串字符中找到“refresh_token”，鼠标拖动选中**冒号后面引号里面**的一长串字符，右键点击即可复制
![](https://s2.loli.net/2022/08/21/LNzU9G5k7eowJRS.png)

16. 将 refresh_token 保存到文本文件
![](https://s2.loli.net/2022/08/21/W6JOvjIprDw81iU.png)

17. 打开 Git Bash，使用下面的命令加密这个文本文件（注意使用强密码！丢失也没关系，再按步骤做一遍就行！）
```bash
gpg --passphrase "【你的强密码】" --batch -o "graphtoken.asc"  -c --armor "【输入文件】"
```

18. 打开输出文件，复制内容
![](https://s2.loli.net/2022/08/21/QKtaHz6xNI5lbZJ.png)

19. 在自己 fork 的仓库，切换到 graphtoken 分支
![](https://s2.loli.net/2022/08/21/NzJRe4E5LSlYVGb.png)

17. 编辑 graphtoken.asc 文件
![](https://s2.loli.net/2022/08/21/Rx4rTsCJ8L2hejA.png)

18. 粘贴刚才复制的内容，直接提交
![](https://s2.loli.net/2022/08/21/fdGqptNy4FZc9Vz.png)

19. 再次到“Secrets”里，创建一个新条目，“Name”为“PASSWORD”，“Value”为刚才加密用的密码
![](https://s2.loli.net/2022/08/21/iupSOXaRbE3Fjxo.png)