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

- 若要同步到滴答清单，按[获取滴答清单 API 口令](./actions-persis.md#获取滴答清单-api-口令)的步骤创建口令，然后将它填入 `DidaApiToken`。

```json
{"CanvasToken":"这里填上你的 Canvas 令牌","DidaApiToken":"这里填上你的滴答清单 API 口令"}
```

从旧版本升级时，将本地 `token.json` 里的 `DidaCredential` 配置替换为 `DidaApiToken`。Windows 图形界面的滴答同步入口读取环境变量 `DIDA_API`。请妥善保存口令，避免将其提交到仓库。

#### 滴答清单本地索引

本地同步会将加密索引保存到当前用户应用数据目录下的 `TodoSync/dida-source-index.bin`。[任务来源的识别规则](./actions-persis.md#滴答清单任务来源与索引)与 GitHub Actions 相同。如果后来给已跳过的任务补写了来源评论，删除该文件后再运行，程序就会重新读取评论。

若滴答 Open API 返回 HTTP 401 或 403，请检查口令是否有效，并确认对应清单允许当前账号写入。

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

按同步目标填写程序和参数。路径含空格时请加引号。
- 同步到 Microsoft Todo，程序为 `wscript.exe`，参数为 VBS 路径。
```
"C:\Users\Public\Download\TodoSync.Local\TodoSync-Todo.vbs"
```
- 同步到滴答清单，程序为 `TodoSynchronizer.CLI.exe`，参数为 `-local -dida`。

#### Linux
下面以 crontab 服务为例展示 Linux 配置定时任务

16. 终端运行命令 `crontab -e`，打开文本编辑器

17. 光标移动到新的一行，输入
- 同步到 Microsoft Todo
```
0 * * * * /path_to_your_program/TodoSynchronizer.CLI -local
```
- 同步到 滴答清单
```
0 * * * * /path_to_your_program/TodoSynchronizer.CLI -local -dida
```

![](https://s2.loli.net/2022/10/17/tzHrZnBcJ94TQVF.png)

18. 按提示保存退出，输入 `crontab -l` 检查是否正确保存

![](https://s2.loli.net/2022/10/17/7BKLxnOj5gtIrol.png)

19. 定时任务配置完毕，定时任务将在每小时的第零分钟执行。建议第一次时检查程序目录下的 `.log` 文件查看是否成功执行（若没有 `.log` 文件，则可能配置有误）
