# Ping9719.MindeoScanner

### 民德扫码器的实现。
##### Mindeo Scanner. 
#

### 下载包 [download、install]
```CSharp
Install-Package Ping9719.MindeoScanner
```
#

### 列子:[ensample code:]
```CSharp
WindowsFormsApp1/Form1.cs
```
#

### 开始使用 [How To Use]
#### 连接 [connect]
```CSharp
using Ping9719.MindeoScanner;

ScannerCode scannerCode = new ScannerCode();
scannerCode.Open("COM1");
```

#### 方法 [method]
```CSharp
//建议先恢复出厂模式，在设置为主机模式
scannerCode.ReadHostOne();//在主机模式下执行一次
scannerCode.ReadHostFor();//在主机模式下一直执行，直到扫描到物品
```

#### 事件 [event]
```CSharp
scannerCode.ScanMess+=...//扫描到的所有消息
```

#
### 版本记录：[version history]
###### *表示部分代码可能与前版本不兼容 [*For some code is incompatible with previous versions]
## v1.2.0
###### 1.支持发送自定义信息 [Supports sending custom messages]
###### 2.支持取消扫描 [Canceling scan]
## v1.1.0
###### 1.公开串口 [Public port]
###### 2.打开串口可以设置更多参数 [Open port you can set more]
## v1.0.0
###### 1.民德扫码器基础消息和主机模式的实现 [Minde scanner basic message and host mode implementation]

