# 架构说明 - DeviceControlSoftware 集成

## ✅ 已修复的继承问题

### 问题
之前的 SequenceItem 类没有继承 DeviceControlSoftware 的基类，导致无法与 SequenceController 正确集成。

### 修复内容

#### 1. SequenceItem 类继承

所有 SequenceItem 现在正确继承 `DeviceControlSoftware.SequenceItem`:

```csharp
// ❌ 之前 (错误)
public class LoadTechniqueSequenceItem
{
    public string ID { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public void Initialize(object context) { }
    public int Execute(object context) { }
    public void Deinitialize(object context) { }
}

// ✅ 现在 (正确)
public class LoadTechniqueSequenceItem : SequenceItem
{
    // ID 和 Properties 从基类继承
    public override void Initialize(Sequence.StateContext context) { }
    public override Sequence.ResultTypes Execute(Sequence.StateContext context) { }
    public override void Deinitialize(Sequence.StateContext context) { }
}
```

#### 2. 方法签名修正

| 方法 | 之前 | 现在 |
|------|------|------|
| Initialize | `void Initialize(object context)` | `void Initialize(Sequence.StateContext context)` |
| Execute | `int Execute(object context)` | `Sequence.ResultTypes Execute(Sequence.StateContext context)` |
| Deinitialize | `void Deinitialize(object context)` | `void Deinitialize(Sequence.StateContext context)` |

#### 3. 返回值类型

Execute 方法现在返回正确的枚举类型：

```csharp
// ❌ 之前
return 0;  // ResultTypes.Next
return -1; // ResultTypes.Error

// ✅ 现在
return Sequence.ResultTypes.Next;
return Sequence.ResultTypes.Error;
```

#### 4. StateContext 的使用

通过 StateContext 可以访问：

```csharp
public override Sequence.ResultTypes Execute(Sequence.StateContext context)
{
    // 访问设备
    var device = context.SequenceDispatcher.Devices["ECLabDevice"];
    
    // 访问设备属性
    if (device.GetProperty("DeviceId") is int deviceId)
    {
        _deviceId = deviceId;
    }
    
    // 访问其他 SequenceItem 的属性
    var voltage = GetProperty(":get_data.Voltage", context);
    
    // 访问设备属性
    var state = GetProperty("@ECLabDevice.ChannelState", context);
    
    // 访问 Values 字典
    var count = GetProperty("#MeasurementCount", context);
    
    // 访问输入参数
    var current = GetProperty("$[MethodParameter].Current_step", context);
    
    return Sequence.ResultTypes.Next;
}
```

#### 5. ECLabSequenceDispatcher 继承

```csharp
// ❌ 之前
public class ECLabSequenceDispatcher
{
    public string Initialize(string parameter) { }
    // ...
}

// ✅ 现在
public class ECLabSequenceDispatcher : SequenceDispatcher
{
    protected override string Name => "ECLabSystem";
    
    protected override void Setup()
    {
        // 注册 SequenceItem 创建器
        AddSequenceCreator("LoadTechnique", 
            props => new LoadTechniqueSequenceItem { Properties = props });
        AddSequenceCreator("StartChannel", 
            props => new StartChannelSequenceItem { Properties = props });
        AddSequenceCreator("StopChannel", 
            props => new StopChannelSequenceItem { Properties = props });
    }
    
    public override void EntryViewModel(IViewModelMenuHost menu)
    {
        // 如果需要 UI 集成
    }
}
```

## 🔗 基类架构

### DeviceControlSoftware.SequenceItem (抽象基类)

```csharp
public abstract class SequenceItem
{
    public string ID { get; set; }
    public string Type { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    
    public abstract void Initialize(Sequence.StateContext context);
    public abstract void Deinitialize(Sequence.StateContext context);
    public abstract Sequence.ResultTypes Execute(Sequence.StateContext context);
    
    // 辅助方法
    protected object? GetProperty(string expr, Sequence.StateContext context);
    protected void SetProperty(string expr, Sequence.StateContext context, object value);
}
```

### 属性引用语法

基类提供了四种属性引用语法：

1. **`:ItemID.Property`** - 引用其他 SequenceItem 的属性
   ```csharp
   var voltage = GetProperty(":get_data.LastVoltage", context);
   ```

2. **`@DeviceID.Property`** - 引用设备属性
   ```csharp
   var state = GetProperty("@ECLabDevice.ChannelState", context);
   ```

3. **`#VariableName`** - 引用 Sequence.Values 字典
   ```csharp
   var count = GetProperty("#MeasurementCount", context);
   SetProperty("#MeasurementCount", context, count + 1);
   ```

4. **`$[MethodParameter].Path`** - 引用输入参数 (JSON)
   ```csharp
   var current = GetProperty("$[MethodParameter].Current_step", context);
   ```

### Sequence.ResultTypes 枚举

```csharp
public enum ResultTypes
{
    Next,    // 继续执行下一个 item
    Error,   // 错误，序列终止
    Jump,    // 跳转到指定 item (配合 JumpSequenceItem)
    End      // 正常结束序列
}
```

## 🏗️ 集成流程

### 1. 项目引用

在 `Biologic.csproj` 中添加：

```xml
<ItemGroup>
  <ProjectReference Include="..\common\DeviceControlSoftware\DeviceControlSoftware.csproj" />
</ItemGroup>
```

### 2. 注册 SequenceItem

在 `ECLabSequenceDispatcher.Setup()` 中：

```csharp
protected override void Setup()
{
    AddSequenceCreator("LoadTechnique", props => new LoadTechniqueSequenceItem { Properties = props });
    AddSequenceCreator("StartChannel", props => new StartChannelSequenceItem { Properties = props });
    AddSequenceCreator("StopChannel", props => new StopChannelSequenceItem { Properties = props });
}
```

### 3. JSON 序列定义

SequenceController 会自动加载 JSON 并实例化 SequenceItem：

```json
{
  "Name": "run-cp-technique",
  "Items": [
    {
      "ID": "load_cp",
      "Type": "LoadTechnique",
      "Properties": {
        "TechniqueFile": "cp.ecc",
        "Parameters": {
          "Current_step": "$[MethodParameter].Current_step"
        }
      }
    },
    {
      "ID": "start",
      "Type": "StartChannel"
    }
  ]
}
```

### 4. 执行流程

```
1. SequenceController.Load("run-cp-technique.json")
   └─> 反序列化 JSON
   └─> 调用 Creator["LoadTechnique"](props)
   └─> 创建 LoadTechniqueSequenceItem 实例

2. OPC UA Method Call: RunCPTechnique({ "Current_step": 0.001 })
   └─> SequenceController.Execute("run-cp-technique", inputJson)
   └─> 创建 StateContext
   └─> 遍历 Items:
       ├─> LoadTechniqueSequenceItem.Initialize(context)
       ├─> LoadTechniqueSequenceItem.Execute(context)
       │   └─> GetProperty("$[MethodParameter].Current_step") → 0.001
       │   └─> BL_LoadTechnique(deviceId, channel, "cp.ecc", params)
       │   └─> return ResultTypes.Next
       ├─> StartChannelSequenceItem.Initialize(context)
       └─> StartChannelSequenceItem.Execute(context)
           └─> BL_StartChannel(deviceId, channel)
           └─> return ResultTypes.Next
```

## 📚 参考实现

### dcs-dispenser 示例

查看参考实现：
- `dcs-dispenser\DispensingSequenceDispatcher.cs` - Dispatcher 示例
- `dcs-dispenser\SequenceItems\*.cs` - SequenceItem 实现示例
- `dcs-dispenser\Sequences\*.json` - JSON 序列定义

### 常用 SequenceItem 模式

```csharp
public class MySequenceItem : SequenceItem
{
    private string _myProperty = string.Empty;
    
    public override void Initialize(Sequence.StateContext context)
    {
        // 从 Properties 读取配置
        _myProperty = Properties?.GetValueOrDefault("MyProperty")?.ToString() ?? "default";
        
        // 解析属性引用 (: @ # $)
        if (_myProperty.StartsWith("$") || _myProperty.StartsWith("@"))
        {
            var value = GetProperty(_myProperty, context);
            _myProperty = value?.ToString() ?? "default";
        }
    }
    
    public override Sequence.ResultTypes Execute(Sequence.StateContext context)
    {
        try
        {
            // 执行操作
            // ...
            
            // 保存结果供其他 item 使用
            if (Properties != null)
            {
                Properties["LastResult"] = result;
            }
            
            return Sequence.ResultTypes.Next;
        }
        catch (Exception ex)
        {
            // 记录错误
            Console.WriteLine($"Error: {ex.Message}");
            return Sequence.ResultTypes.Error;
        }
    }
    
    public override void Deinitialize(Sequence.StateContext context)
    {
        // 清理资源
    }
}
```

## ✨ 优势

通过正确继承基类，获得以下优势：

1. **统一架构** - 与其他设备系统 (dispenser, etc.) 保持一致
2. **自动管理** - SequenceController 自动处理生命周期
3. **属性引用** - 使用统一的语法访问设备、变量、参数
4. **JSON 驱动** - 工作流定义与代码分离
5. **错误处理** - 统一的错误传播机制
6. **并发安全** - StateContext 确保线程安全

## 🚀 下一步

1. ✅ SequenceItem 继承已修复
2. ✅ ECLabSequenceDispatcher 继承已修复
3. ⬜ 添加项目引用到 DeviceControlSoftware
4. ⬜ 实现 WaitForCompletionSequenceItem
5. ⬜ 实现 GetDataSequenceItem
6. ⬜ 编译 OPC UA NodeSet
7. ⬜ 集成到 Open62541Wrapper

---

**修改完成！** 现在 Biologic 项目的架构与 dcs-dispenser 完全一致。🎉
