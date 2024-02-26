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

            public const string productNameErrorMessage = "Product name require length between 1 and 1024.";
            public const string productDescriptionErrorMessage = $"Product description require length between 1 and 2147483647 symbols.";
            public const string productPriceErrorMessage = "Price should be between 0 and 2147483647";
            public const string productSKUErrorMessage = $"SKU length should be between 1 and 64";
            public const string productQuantityErrorMessage = "Product quantity should be between 0 and 2147483647";
        }



        public const int productCategoryMinLength = 1;
        public const int productCategoryMaxLength = 64;

        public static class ProductImage
        {
            public const int nameMinLength = 1;
            public const int nameMaxLength = 256;

        }

        public static class Promotion
        {
            public const int nameMinLength = 1;
            public const int nameMaxLength = 256;
            public const int descriptionMinLength = 1;
            public const int descriptionMaxLength = int.MaxValue;
            public const int discountMinRate = 0;
            public const int discountMaxRate = 100;
        }

        public const int shippingMethodNameMinLength = 1;
        public const int shippingMethodNameMaxLength = 64;
        public const int shippingMethodMinPrice = 0;
        public const double shippingMethodMaxPrice = 1024 * 2;

        public const double orderlTotalMinValue = 0;
        public const double orderTotalMaxValue = double.MaxValue;

        public const int shoppingCartItemQuantityMinValue = 1;
        public const int shoppingCartItemQuantityMaxValue = int.MaxValue;

        public const int firstAndLastNameMinLength = 1;
        public const int firstAndLastNameMaxLength = 64;

        public const double ratingValueMin = 1;
        public const double ratingValueMax = 5;
        public const int commentMinLength = 1;
        public const int commentMaxLength = 4096;

        public const int variationOptionValueMinLength = 1;
        public const int variationOptionValueMaxLength = 512;
    }
}
