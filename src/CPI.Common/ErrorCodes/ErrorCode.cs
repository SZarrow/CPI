using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Lotus.Core;

namespace CPI.Common
{
    /// <summary>
    /// 通用错误码，取值范围：[0,2000]
    /// </summary>
    [Serializable]
    public sealed class ErrorCode
    {
        /// <summary>
        /// 表示成功
        /// </summary>
        [Description("成功")]
        public const Int32 SUCCESS = 0;
        /// <summary>
        /// 表示失败
        /// </summary>
        [Description("失败")]
        public const Int32 FAILURE = 1;
        /// <summary>
        /// 表示空引用
        /// </summary>
        [Description("未将对象引用到对象的实例")]
        public const Int32 NULL_REFERENCE = 1001;
        /// <summary>
        /// 表示无效的参数
        /// </summary>
        [Description("无效的参数")]
        public const Int32 INVALID_ARGUMENT = 1002;
        /// <summary>
        /// 表示无效的转换
        /// </summary>
        [Description("无效的转换")]
        public const Int32 INVALID_CAST = 1003;
        /// <summary>
        /// 表示无效的操作
        /// </summary>
        [Description("无效的操作")]
        public const Int32 INVALID_OPERATION = 1004;
        /// <summary>
        /// 表示未授权的访问
        /// </summary>
        [Description("未授权的访问")]
        public const Int32 UNAUTHORIZED_ACCESS = 1005;
        /// <summary>
        /// 表示重复提交
        /// </summary>
        [Description("重复提交")]
        public const Int32 SUBMIT_REPEAT = 1006;
        /// <summary>
        /// 表示远程未返回任何数据
        /// </summary>
        [Description("远程未返回任何数据")]
        public const Int32 REMOTE_RETURN_NOTHING = 1007;
        /// <summary>
        /// 表示签名失败
        /// </summary>
        [Description("签名失败")]
        public const Int32 SIGN_FAILED = 1008;
        /// <summary>
        /// 表示序列化失败
        /// </summary>
        [Description("序列化失败")]
        public const Int32 SERIALIZE_FAILED = 1009;
        /// <summary>
        /// 更新数据失败
        /// </summary>
        [Description("更新数据失败")]
        public const Int32 DB_UPDATE_FAILED = 1010;
        /// <summary>
        /// 表示反序列化失败
        /// </summary>
        [Description("反序列化失败")]
        public const Int32 DESERIALIZE_FAILED = 1011;
        /// <summary>
        /// 依赖的API调用失败
        /// </summary>
        [Description("依赖的API调用失败")]
        public const Int32 DEPENDENT_API_CALL_FAILED = 1012;
        /// <summary>
        /// RSA密钥未找到
        /// </summary>
        [Description("RSA密钥未找到")]
        public const Int32 RSAKEY_NOT_FOUND = 1013;
        /// <summary>
        /// 签名验签失败
        /// </summary>
        [Description("签名验签失败")]
        public const Int32 SIGN_VERIFY_FAILED = 1014;
        /// <summary>
        /// 不支持该方法
        /// </summary>
        [Description("不支持该方法")]
        public const Int32 METHOD_NOT_SUPPORT = 1015;
        /// <summary>
        /// 调用失败
        /// </summary>
        [Description("方法调用失败")]
        public const Int32 METHOD_INVOKE_FAILED = 1016;
        /// <summary>
        /// 外部交易编号重复
        /// </summary>
        [Description("外部交易编号重复")]
        public const Int32 OUT_TRADE_NO_EXISTED = 1017;
        /// <summary>
        /// 外部交易编号不存在
        /// </summary>
        [Description("外部交易编号不存在")]
        public const Int32 OUT_TRADE_NO_NOT_EXIST = 1018;
        /// <summary>
        /// BizContent参数解析失败
        /// </summary>
        [Description("BizContent参数解析失败")]
        public const Int32 BIZ_CONTENT_DESERIALIZE_FAILED = 1019;
        /// <summary>
        /// 请求超时
        /// </summary>
        [Description("请求超时")]
        public const Int32 REQUEST_TIMEOUT = 1020;
        /// <summary>
        /// 未知错误
        /// </summary>
        [Description("未知错误")]
        public const Int32 UNKNOW_ERROR = 1021;
        /// <summary>
        /// 信息已存在
        /// </summary>
        [Description("信息已存在")]
        public const Int32 INFO_EXISTED = 1022;
        /// <summary>
        /// 信息不存在
        /// </summary>
        [Description("信息不存在")]
        public const Int32 INFO_NOT_EXIST = 1023;
        /// <summary>
        /// 数据查询失败
        /// </summary>
        [Description("数据查询失败")]
        public const Int32 DB_QUERY_FAILED = 1024;
        /// <summary>
        /// XML元素不存在
        /// </summary>
        [Description("XML元素不存在")]
        public const Int32 XML_ELEMENT_NOT_EXIST = 1025;
        /// <summary>
        /// 解码失败
        /// </summary>
        [Description("解码失败")]
        public const Int32 DECODE_FAILED = 1026;
        /// <summary>
        /// 读取响应数据失败
        /// </summary>
        [Description("读取响应数据失败")]
        public const Int32 RESPONSE_READ_FAILED = 1027;
        /// <summary>
        /// XML解析失败
        /// </summary>
        [Description("XML解析失败")]
        public const Int32 XML_PARSE_FAILED = 1028;
        /// <summary>
        /// 加密失败
        /// </summary>
        [Description("加密失败")]
        public const Int32 ENCRYPT_FAILED = 1029;
        /// <summary>
        /// 解密失败
        /// </summary>
        [Description("解密失败")]
        public const Int32 DECRYPT_FAILED = 1030;
        /// <summary>
        /// 任务执行失败
        /// </summary>
        [Description("任务执行失败")]
        public const Int32 TASK_EXECUTE_FAILED = 1031;
        /// <summary>
        /// 编码失败
        /// </summary>
        [Description("编码失败")]
        public const Int32 ENCODE_FAILED = 1032;
    }
}
