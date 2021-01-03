using System.Text;

namespace RedundantFileRemover {
    static class StringUtils {

        public static string RemoveSpecialCharacters(this string str, params char[] excepts) {
            var sb = new StringBuilder();

            foreach (char c in str) {
                bool allow = false;

                if (excepts.Length != 0) {
                    foreach (char ex in excepts) {
                        if (ex == c) {
                            allow = true;
                            break;
                        }
                    }
                }

                if (allow || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
