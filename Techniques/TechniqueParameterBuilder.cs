using Biologic.Native;

namespace Biologic.Techniques;

/// <summary>
/// Helper class to build EccParams structures from TechniqueParameterBase objects
/// </summary>
public static class TechniqueParameterBuilder
{
  /// <summary>
  /// Build EccParams from technique parameters
  /// </summary>
  /// <param name="parameters">Technique parameters object</param>
  /// <returns>EccParams structure ready for BL_LoadTechnique</returns>
  public static EccParams BuildEccParams(TechniqueParameterBase parameters)
  {
    ArgumentNullException.ThrowIfNull(parameters);

    var paramDict = parameters.BuildParameters();
    var paramArray = new EccParam[paramDict.Count];
    int index = 0;

    foreach (var kvp in paramDict)
    {
      paramArray[index] = CreateEccParam(kvp.Key, kvp.Value, index);
      index++;
    }

    var eccParams = new EccParams
    {
      len = paramArray.Length,
      pParams = IntPtr.Zero
    };

    if (paramArray.Length > 0)
    {
      int paramSize = System.Runtime.InteropServices.Marshal.SizeOf<EccParam>();
      eccParams.pParams = System.Runtime.InteropServices.Marshal.AllocHGlobal(paramSize * paramArray.Length);

      for (int i = 0; i < paramArray.Length; i++)
      {
        IntPtr ptr = IntPtr.Add(eccParams.pParams, i * paramSize);
        System.Runtime.InteropServices.Marshal.StructureToPtr(paramArray[i], ptr, false);
      }
    }

    return eccParams;
  }

  /// <summary>
  /// Free unmanaged memory allocated for EccParams
  /// </summary>
  public static void FreeEccParams(ref EccParams eccParams)
  {
    if (eccParams.pParams != IntPtr.Zero)
    {
      System.Runtime.InteropServices.Marshal.FreeHGlobal(eccParams.pParams);
      eccParams.pParams = IntPtr.Zero;
      eccParams.len = 0;
    }
  }

  /// <summary>
  /// Create single EccParam from key-value pair
  /// </summary>
  private static EccParam CreateEccParam(string label, object value, int index)
  {
    var param = new EccParam
    {
      ParamStr = System.Text.Encoding.ASCII.GetBytes(label.PadRight(64, '\0')),
      ParamIndex = index
    };

    switch (value)
    {
      case bool boolValue:
        param.ParamType = (int)PARAM_TYPE.PARAM_BOOLEAN;
        param.ParamVal = boolValue ? 1 : 0;
        break;

      case int intValue:
        param.ParamType = (int)PARAM_TYPE.PARAM_INT;
        param.ParamVal = intValue;
        break;

      case float floatValue:
        param.ParamType = (int)PARAM_TYPE.PARAM_SINGLE;
        param.ParamVal = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
        break;

      case double doubleValue:
        param.ParamType = (int)PARAM_TYPE.PARAM_SINGLE;
        float f = (float)doubleValue;
        param.ParamVal = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
        break;

      default:
        throw new ArgumentException($"Unsupported parameter type: {value.GetType()}", nameof(value));
    }

    return param;
  }

  /// <summary>
  /// Build EccParams using ECLibNative helper methods (alternative approach)
  /// </summary>
  public static EccParams BuildEccParamsUsingNative(TechniqueParameterBase parameters)
  {
    ArgumentNullException.ThrowIfNull(parameters);

    var paramDict = parameters.BuildParameters();
    var paramArray = ECLibNative.CreateEccParamArray(paramDict.Count);
    int index = 0;

    foreach (var kvp in paramDict)
    {
      EccParam param;
      switch (kvp.Value)
      {
        case bool boolValue:
          ECLibNative.BL_DefineBoolParameter(kvp.Key, boolValue, index, out param);
          break;

        case int intValue:
          ECLibNative.BL_DefineIntParameter(kvp.Key, intValue, index, out param);
          break;

        case float floatValue:
          ECLibNative.BL_DefineSglParameter(kvp.Key, floatValue, index, out param);
          break;

        case double doubleValue:
          ECLibNative.BL_DefineSglParameter(kvp.Key, (float)doubleValue, index, out param);
          break;

        default:
          throw new ArgumentException($"Unsupported parameter type: {kvp.Value.GetType()}");
      }

      paramArray[index] = param;
      index++;
    }

    var eccParams = new EccParams
    {
      len = paramArray.Length,
      pParams = IntPtr.Zero
    };

    if (paramArray.Length > 0)
    {
      int paramSize = System.Runtime.InteropServices.Marshal.SizeOf<EccParam>();
      eccParams.pParams = System.Runtime.InteropServices.Marshal.AllocHGlobal(paramSize * paramArray.Length);

      for (int i = 0; i < paramArray.Length; i++)
      {
        IntPtr ptr = IntPtr.Add(eccParams.pParams, i * paramSize);
        System.Runtime.InteropServices.Marshal.StructureToPtr(paramArray[i], ptr, false);
      }
    }

    return eccParams;
  }
}
