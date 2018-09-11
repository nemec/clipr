using System;
using System.IO;

namespace clipr.Validation
{
    public class DirectoryExistsAttribute : ValidationAttribute
    {
        public override bool CanHandleType(Type type, out string errorMessage)
        {
            if(type != typeof(DirectoryInfo) && type != typeof(string))
            {
                errorMessage = "Only System.IO.DirectoryInfo or a System.String can be validated by the DirectoryExistsAttribute.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        public override bool ValidateMember(object member, out Exception error)
        {
            if(member == null)
            {
                error = new ArgumentException("Member cannot be null in DirectoryExistsAttribute", "member");
                return false;
            }
            DirectoryInfo f;

            if(member is string s)
            {
                try
                {
                    f = new DirectoryInfo(s);
                }
                catch (ArgumentException e)
                {
                    error = e;
                    return false;
                }
            }
            else if(member is DirectoryInfo)
            {
                f = member as DirectoryInfo;
            }
            else
            {
                error = new ArgumentException(String.Format(
                    "Unsupported type '{0}' in DirectoryExistsAttribute", member.GetType()), "member");
                return false;
            }

            if (!f.Exists)
            {
                error = new FormatException(String.Format(
                    "Directory '{0}' does not exist.", f));
                return false;
            }
            error = null;
            return true;
        }
    }
}
