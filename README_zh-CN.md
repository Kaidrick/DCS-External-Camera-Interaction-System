# DCS外部摄像机交互系统
这是一个非常初期的摄像机控制的概念的实现，使用CSharp WPF。


**注意**

你随时可以根据以下步骤来移除这个软件：
1. 删除该软件本身及所在文件夹
2. 从保存的游戏中的DCS目录中移除DCS-AECIS.lua
3. **从Export.lua中移除下列代码**

```
-- AECIS

local dcsAECIS=require('lfs');dofile(dcsAECIS.writedir()..[[Scripts\DCS-AECIS.lua]])
```

**已知的问题**
* 某些情况下，Export.lua载入DCS-AECIS.lua文件后，用户的DCS模拟帧率降低至1fps。
* 有些用户报告摄像机镜头无法旋转，无论是通过虚拟摇杆还是鼠标控制
* 弹出的对话框可能导致鼠标和键盘卡顿
* 与DCS的TCP连接丢失可能导致鼠标和键盘输入卡顿
* 启用键鼠控制可能回引起严重的鼠标和键盘输入卡顿

开发端未遇到上述的任何问题，需要更多的信息。


**如何安装和使用**

你需要在系统盘中的\用户\\\<你的用户名>\保存的游戏\DCS\.\<分支名称（如openbeta）\>\Scripts中加入下列文件。
* DCS-AECIS.lua

如果这个文件夹中没有Export.lua文件，你需要创建一个新的Export.lua
在文件管理器中，右键选择新建 > 文本文档。将这个文件重命名为Export.lua，注意，如果你的文件浏览器中不显示扩展名，重命名默认会是Export.lua.txt，则无法被DCS读取。

在这个文件的最后一行中写入如下代码：


`-- AECIS`

`local dcsAECIS=require('lfs');dofile(dcsAECIS.writedir()..[[Scripts\DCS-AECIS.lua]])`

这会让DCS在单独的命名空间/lua表中载入AECIS。
进入一个任务后，按F11或左Ctrl+F11来切换到自由视角模式。按“连接”来尝试连接到DCS。如果按钮变为“断开连接”，则表示TCP连接已经正确建立且Export.lua导出正常。

**按住左Ctrl并用鼠标拖动窗体来改变窗口位置。**

目前不熟悉如何在WPF中动态实现本地化。中文版本暂时在另一个Release中。
![User Interface](overview_zh-CN.png)
