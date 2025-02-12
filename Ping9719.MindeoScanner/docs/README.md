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