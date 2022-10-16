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

### 二、获取 Graph 令牌

下面以 Windows 系统为例，借助“Graph 认证辅助工具”（在本仓库开源）进行配置。如有其它需求，可参考[手动配置 Graph Token](./graph-token-manually.md)

5.  在 [Releases](../../../releases) 页面下载“TodoSynchronizer.QuickTool.exe”，打开。

6.  点击“获取 Graph 认证”，跳转到浏览器

7.  授权 MyTodoApp（务必核对权限是否与图中一致）

![](https://s2.loli.net/2022/08/21/JiYnCMUPshc5RGd.png)

8. 完成后，回到程序，**点击下面的蓝色的“直接复制 Token”**

![](https://s2.loli.net/2022/10/16/xh9iu23F1lvjftD.png)

### 三、创建本地 Token 存储文件
9. 在合适的位置新建一个文本文件（要求 TodoSync 程序可读写此文件）

![](https://s2.loli.net/2022/10/16/sulWqNQSCxMVD3j.png)

10. 输入以下内容，并填上前面获取的两个令牌
```
{"CanvasToken":"这里填上你的 Canvas 令牌","GraphToken":"这里填上你的 Graph 令牌"}
```
![](https://s2.loli.net/2022/10/16/B2V95rqDgX7UAR3.png)

### 四、配置定时任务
#### Windows
11. 搜索“任务计划程序”，打开

![](https://s2.loli.net/2022/10/16/7eg1TNXORzpkds4.png)

12. 在“任务计划程序库”或其任一子文件夹内**创建任务**（注意不是“创建基本任务”）

![](https://s2.loli.net/2022/10/16/UCWtBlhPgapVFvI.png)

13. 按图中配置

![](https://s2.loli.net/2022/10/16/AjFl7RwaMiV6SWg.png)
![](https://s2.loli.net/2022/10/16/HXPyeEQ75K3TRtM.png)
![](https://s2.loli.net/2022/10/16/1jZAh4k5wFsDzVO.png)

其中程序为从 Releases 界面下载的 TodoSynchronizer.CLI.exe，参数如下
```
-configfile 你的config文件路径 -tokenfile 你的token文件路径
```
