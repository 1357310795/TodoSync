## 配置步骤
### 一、获取 Canvas 令牌

1. 打开 Canvas，进入“设置”页面

![](https://s2.loli.net/2022/08/21/bdnaM9jLhvCI4i3.png)

2. 点击“创建新访问许可证”

![](https://s2.loli.net/2022/08/21/FheNU1Rlz7X5cgS.png)

3. “用途”随便填一个，“过期”建议留空，点击“生成令牌”

![](https://s2.loli.net/2022/08/21/riymJ4DqvI2ZAPb.png)

4. 复制生成的令牌**备用**

![](https://s2.loli.net/2022/08/21/Eyej95vY3cCsVZT.png)

### 二、获取 Graph 令牌（若要同步到 Microsoft Todo）
#### Windows
推荐借助“Graph 认证辅助工具”（在本仓库开源）进行配置。

5.  在 [Releases](../../../releases) 页面下载“TodoSynchronizer.QuickTool.exe”，打开。

6.  点击“获取 Graph 认证”，跳转到浏览器

7.  授权 MyTodoApp（务必核对权限是否与图中一致）

![](https://s2.loli.net/2022/08/21/JiYnCMUPshc5RGd.png)

8. 完成后，回到程序，**点击下面的蓝色的“直接复制 Token”**

![](https://s2.loli.net/2022/10/16/xh9iu23F1lvjftD.png)

#### Linux 
Linux系统可参考[手动配置 Graph Token](./graph-token-manually.md)**（到第六步为止！！！只需要 refresh_token ！！！不用加密！！！）**

### 三、更新本地 Token 存储文件
9. 在 [Releases](../../../releases) 界面下载对应系统（Windows/Linux）的本地运行版本程序包。

![](https://s2.loli.net/2022/10/17/JZVb7oAkfF6veUj.png)

10. 解压程序包到合适的位置（要求程序可读写此位置）

11. 使用**文本编辑器**打开程序包目录下的 `token.json` 文件

![](https://s2.loli.net/2022/10/20/Re58q2uTV1MaPB4.png)

12. 填上前面获取的令牌
- 若要同步到 Microsoft Todo，按以下格式填写
```
{"CanvasToken":"这里填上你的 Canvas 令牌","GraphToken":"这里填上你的 Graph 令牌"}
```

- 若要同步到滴答清单，按以下格式填写
```
{"CanvasToken":"这里填上你的 Canvas 令牌","DidaCredential":{"phone":"这里填上你的滴答清单登录手机号","password":"这里填上你的滴答清单登录密码"}}
```

![](https://s2.loli.net/2022/10/16/B2V95rqDgX7UAR3.png)

### 四、配置定时任务
#### Windows
13. 搜索“任务计划程序”，打开

![](https://s2.loli.net/2022/10/16/7eg1TNXORzpkds4.png)

14. 在“任务计划程序库”或其任一子文件夹内**创建任务**（注意不是“创建基本任务”）

![](https://s2.loli.net/2022/10/16/UCWtBlhPgapVFvI.png)

15. 按图中配置

![](https://s2.loli.net/2022/10/16/AjFl7RwaMiV6SWg.png)
![](https://s2.loli.net/2022/10/17/37nD4mpM6NabzeW.png)
![](https://s2.loli.net/2022/10/17/Cjl3nehm2VvFGcR.png)

其中程序名称为 `wscript.exe`，参数为程序包中 vbs 文件的路径（建议包含引号），例如
- 同步到 Microsoft Todo
```
"C:\Users\Public\Download\TodoSync.Local\TodoSync-Todo.vbs"
```
- 同步到 滴答清单
```
"C:\Users\Public\Download\TodoSync.Local\TodoSync-Dida.vbs"
```
#### Linux
下面以 crontab 服务为例展示 Linux 配置定时任务

16. 终端运行命令 `crontab -e`，打开文本编辑器

17. 光标移动到新的一行，输入
- 同步到 Microsoft Todo
```
0 * * * * /path_to_your_program/TodoSyncronizer.CLI -local
```
- 同步到 滴答清单
```
0 * * * * /path_to_your_program/TodoSyncronizer.CLI -local -didacredentialfile 114514
```

![](https://s2.loli.net/2022/10/17/tzHrZnBcJ94TQVF.png)

18. 按提示保存退出，输入 `crontab -l` 检查是否正确保存

![](https://s2.loli.net/2022/10/17/7BKLxnOj5gtIrol.png)

19. 定时任务配置完毕，定时任务将在每小时的第零分钟执行。建议第一次时检查程序目录下的 `.log` 文件查看是否成功执行（若没有 `.log` 文件，则可能配置有误）
