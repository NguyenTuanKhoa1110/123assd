namespace W3_test.Domain.Models
{
    public static class Permissions
    {

        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
            public const string ManageRoles = "Permissions.Users.ManageRoles";
        }

        public static class Books
        {
            public const string Create = "Permissions.Books.Create";
            public const string View = "Permissions.Books.View";
            public const string Update = "Permissions.Books.Update";
            public const string Delete = "Permissions.Books.Delete";
        }

    }



}
