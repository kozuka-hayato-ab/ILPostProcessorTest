using UnityEngine;

namespace Settings
{
    public class ProjectDefine : MonoBehaviour
    {
        /// <summary>
        /// 書き換えたい文字列
        /// </summary>
        private string Value { get; } = "Hello World";

        void Start()
        {
            Debug.Log(Value);
        }
    }
}
