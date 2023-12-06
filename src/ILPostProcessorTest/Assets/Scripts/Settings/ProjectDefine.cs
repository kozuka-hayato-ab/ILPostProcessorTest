using System;

namespace Settings
{
    public class ProjectDefine
    {
        /// <summary>
        /// 書き換えなくてもいい文字列
        /// </summary>
        public string Value2 { get; } = "Hello Japan";
        
        /// <summary>
        /// 書き換えたい文字列
        /// </summary>
        [Rewriting]
        public string Value { get; } = "Hello World";
    }
}
