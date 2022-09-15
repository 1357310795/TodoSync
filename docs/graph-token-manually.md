### 手动配置 Graph Token

1. 访问[这个链接](https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id=49694ef2-8751-4ac9-8431-8817c27350b4&response_type=code&redirect_uri=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fnativeclient&response_mode=query&scope=Tasks.ReadWrite%20User.Read%20offline_access&state=12345)，登录 Microsoft 账户

![](https://s2.loli.net/2022/08/21/B7Kj6a5tJPXQqSL.png)

2. 授权 MyTodoApp（务必核对权限是否与图中一致）

![](https://s2.loli.net/2022/08/21/JiYnCMUPshc5RGd.png)

1. 完成后，在地址栏找到 code 参数

![](https://s2.loli.net/2022/08/21/nD6NeU3PkhaAs5H.png)

4. 打开终端（不建议使用 Windows Terminal 和 Powershell）

![](https://s2.loli.net/2022/08/21/3DiI6pwYgJFzufk.png)

5. 粘贴以下命令并运行（或者使用 Postman 等工具）

```bash
curl -d "client_id=49694ef2-8751-4ac9-8431-8817c27350b4&scope=Tasks.ReadWrite%20User.Read%20offline_access&redirect_uri=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fnativeclient&grant_type=authorization_code&code=【这里换成你的code！】" https://login.microsoftonline.com/consumers/oauth2/v2.0/token
```

6. 复制“refresh_token”的值

![](https://s2.loli.net/2022/08/21/LNzU9G5k7eowJRS.png)

7. 打开 [CyberChef-CN](https://1357310795.github.io/CyberChef-CN) 或任何你常用的密码学算法工具（需要能够处理 MD5 哈希和自定义 IV 的 AES 加密）。

![](https://s2.loli.net/2022/09/14/hAugaKWXm6YPUnb.png)

8. 左侧搜索“MD5”，拖到“配方”里面，并在右上角的输入框输入用于保护凭证的密钥（强烈建议在密钥中包含中文，密钥无需记忆）

![](https://s2.loli.net/2022/09/14/hAWxOm2p5yI4Vz1.png)

9.  复制 MD5 的结果，把“配方”清空，左侧搜索“AES”，把“AES Encrypt”拖到“配方”里面，再搜索“Base64”，把“文本转 Base64”拖到“AES Encrypt”的下面

![](https://s2.loli.net/2022/09/14/l52AVj6OtWzaHgy.png)

10. 在“AES Encrypt”的“Key”和“IV”粘贴刚刚复制的 MD5 值，“Output”改为“Raw”

![](https://s2.loli.net/2022/09/14/xCd3eu6OlkfGn1a.png)

11. 右上角输入框粘贴 (6) 中得到的 refresh_token，复制输出的内容

![](https://s2.loli.net/2022/09/14/LRZhENcWXA8Vix9.png)

12. 在自己 fork 的仓库，切换到 graphtoken 分支

![](https://s2.loli.net/2022/08/21/NzJRe4E5LSlYVGb.png)

13. 编辑 graphtoken.asc 文件

![](https://s2.loli.net/2022/09/14/6DLgz7mHQdVSnZ2.png)

14. 粘贴刚才复制的内容，直接提交

![](https://s2.loli.net/2022/08/21/fdGqptNy4FZc9Vz.png)

15. 再次到“Secrets”里，创建一个新条目，“Name”为“KEY”，“Value”为刚才输入的密钥

![](https://s2.loli.net/2022/09/14/4akGQOzVLH7nYvC.png)