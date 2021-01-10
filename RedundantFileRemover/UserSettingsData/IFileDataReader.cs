namespace RedundantFileRemover.UserSettingsData {
    internal interface IFileDataReader {

        void Save(YamlDotNet.RepresentationModel.YamlStream stream);

    }
}
