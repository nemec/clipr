using System;
using System.IO;

namespace clipr.Validation
{
    public class FileExistsAttribute : ValidationAttribute
    {
        public override bool CanHandleType(Type type, out string errorMessage)
        {
            if(type != typeof(FileInfo) && type != typeof(string))
            {
                errorMessage = "Only System.IO.FileInfo or a System.String can be validated by the FileExistsAttribute.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        public override bool ValidateMember(object member, out Exception error)
        {
            if(member == null)
            {
                error = new ArgumentException("Member cannot be null in FileExistsAttribute", "member");
                return false;
            }
            FileInfo f;

            if(member is string s)
            {
                try
                {
                    f = new FileInfo(s);
                }
                catch (ArgumentException e)
                {
                    error = e;
                    return false;
                }
            }
            else if(member is FileInfo)
            {
                f = member as FileInfo;
            }
            else
            {
                error = new ArgumentException(String.Format(
                    "Unsupported type '{0}' in FileExistsAttribute", member.GetType()), "member");
                return false;
            }

            if (!f.Exists)
            {
                error = new FormatException(String.Format(
                    "File '{0}' does not exist.", f));
                return false;
            }
            error = null;
            return true;
        }
    }
}
