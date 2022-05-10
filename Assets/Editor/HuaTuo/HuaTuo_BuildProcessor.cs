using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace HuaTuo
{
    public class HuaTuo_BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            
        }
    }
}