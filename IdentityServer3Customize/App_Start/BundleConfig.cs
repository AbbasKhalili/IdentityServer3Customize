using System.Web.Optimization;

namespace IdentityServer3Customize
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }

    }
}