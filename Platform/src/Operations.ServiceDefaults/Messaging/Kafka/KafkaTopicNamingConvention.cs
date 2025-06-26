// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.ServiceDefaults.Messaging.Kafka;

public static class KafkaTopicNamingConvention
{
    public const string VERSION = "v1";

    public static class Accounting
    {
        public const string SERVICE = "accounting";

        public static class Ledger
        {
            public const string AGGREGATE = "ledger";
            public static string Topic => $"{SERVICE}.{AGGREGATE}.{VERSION}";
        }

        public static class Operation
        {
            public const string AGGREGATE = "operation";
            public static string Topic => $"{SERVICE}.{AGGREGATE}.{VERSION}";
        }
    }

    public static class Billing
    {
        public const string SERVICE = "billing";

        public static class Cashier
        {
            public const string AGGREGATE = "cashier";
            public static string Topic => $"{SERVICE}.{AGGREGATE}.{VERSION}";
        }

        public static class Invoice
        {
            public const string AGGREGATE = "invoice";
            public static string Topic => $"{SERVICE}.{AGGREGATE}.{VERSION}";
        }
    }

    public static string GetPartitionKey(string tenantId) => tenantId;
}
