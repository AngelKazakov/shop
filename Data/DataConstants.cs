namespace RandomShop.Data
{
    public static class DataConstants
    {
        public static class Address
        {
            public const int StreetNumberMin = 1;
            public const int StreetNumberMax = 8;
            public const int addressLineMinLength = 4;
            public const int addressLineMaxLength = 128;
            public const int postalCodeMin = 4;
            public const int postalCodeMax = 8;
        }

        public const int countryNameMaxLength = 128;

        public static class OrderLine
        {
            public const int quantityMin = 0;
            public const int quantityMax = int.MaxValue;
            public const int priceMin = 0;
            public const double priceMax = double.MaxValue;
        }

        public static class PaymentType
        {
            public const int statusMaxLength = 64;
            public const int paymentTypeValueMaxLength = 16;
            public const int paymentTypeValueMinLength = 4;
        }

        public static class Product
        {
            public const int nameMinLength = 1;
            public const int nameMaxLength = 1024;
            public const int descriptionMinLength = 1;
            public const int descriptionMaxLength = int.MaxValue;

            public const int productItemSKUMinLength = 1;
            public const int productItemSKUMaxLength = 64;

            public const int productItemQuantityMin = 0;
            public const int productItemQuantityMax = int.MaxValue;

            public const double priceMin = 0;
            public const double priceMax = double.MaxValue;
        }
    }
}
