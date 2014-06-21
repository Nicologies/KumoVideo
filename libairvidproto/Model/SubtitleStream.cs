
using aairvid.Protocol;

namespace aairvid.Model
{
    public class SubtitleStream
    {
        public SubtitleStream()
        {
        }

        public StringValue Language
        {
            get;
            set;
        }

        public string DisplayableLan
        {
            get
            {
                if (Language == null
                    ||string.IsNullOrWhiteSpace(Language.Value)
                    ||Language.Value.ToUpperInvariant() == "UND")
                {
                    return string.Format("Unknow({0})", LoaderId.Value);
                }
                if (Language.Value.ToUpperInvariant() == "DISABLED")
                {
                    return "Disable Sub";
                }
                return string.Format("{0}({1})", Language.Value, LoaderId.Value);
            }
        }

        public StringValue LoaderId
        {
            get;
            set;
        }

        public StringValue Path
        {
            get;
            set;
        }

        public KeyValueBase LoaderData
        {
            get;
            set;
        }

        public RootObj SubtitleInfoFromServer
        {
            get
            {
                var obj = new RootObj(RootObj.EmObjType.SubtitleInfo);

                if (LoaderId == null)
                {
                    LoaderId = new StringValue("loaderId", "");
                }
                obj.Add(LoaderId);

                if (Path == null)
                {
                    Path = new StringValue("path", "");
                }
                obj.Add(Path);

                if (Language == null)
                {
                    Language = new StringValue("language", "");
                }
                obj.Add(Language);

                if (LoaderData == null)
                {
                    LoaderData = new StringValue("loaderData", "");
                }
                obj.Add(LoaderData);
                return obj;
            }
        }
    }
}