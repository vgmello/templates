import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

// Configuration
const GRPC_HOST = 'localhost';
const GRPC_PORT = '4061';
const PROTO_PATH = './protos/cashiers.proto';

async function testGrpcConnection() {
    console.log('Testing gRPC connection to', `${GRPC_HOST}:${GRPC_PORT}`);
    
    try {
        // Load proto definition
        const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
            keepCase: true,
            longs: String,
            enums: String,
            defaults: true,
            oneofs: true,
        });

        const protoDescriptor = grpc.loadPackageDefinition(packageDefinition);
        const CashiersService = protoDescriptor.billing.cashiers.CashiersService;

        // Create client
        const client = new CashiersService(
            `${GRPC_HOST}:${GRPC_PORT}`,
            grpc.credentials.createInsecure()
        );

        console.log('gRPC client created successfully');

        // Test GetCashiers
        console.log('\n--- Testing GetCashiers ---');
        const getCashiersPromise = new Promise((resolve, reject) => {
            client.GetCashiers({ limit: 10, offset: 0 }, (error, response) => {
                if (error) {
                    console.error('GetCashiers error:', error.message);
                    console.error('Error code:', error.code);
                    console.error('Error details:', error.details);
                    reject(error);
                } else {
                    console.log('GetCashiers success:', response);
                    resolve(response);
                }
            });
        });

        await getCashiersPromise;

        // Test CreateCashier
        console.log('\n--- Testing CreateCashier ---');
        const createCashierPromise = new Promise((resolve, reject) => {
            client.CreateCashier({ 
                name: 'Test gRPC User', 
                email: 'test-grpc@example.com' 
            }, (error, response) => {
                if (error) {
                    console.error('CreateCashier error:', error.message);
                    console.error('Error code:', error.code);
                    console.error('Error details:', error.details);
                    reject(error);
                } else {
                    console.log('CreateCashier success:', response);
                    resolve(response);
                }
            });
        });

        const newCashier = await createCashierPromise;

        // Test GetCashier
        if (newCashier && newCashier.cashierId) {
            console.log('\n--- Testing GetCashier ---');
            const getCashierPromise = new Promise((resolve, reject) => {
                client.GetCashier({ id: newCashier.cashierId }, (error, response) => {
                    if (error) {
                        console.error('GetCashier error:', error.message);
                        console.error('Error code:', error.code);
                        console.error('Error details:', error.details);
                        reject(error);
                    } else {
                        console.log('GetCashier success:', response);
                        resolve(response);
                    }
                });
            });

            await getCashierPromise;
        }

        console.log('\n✅ All gRPC tests passed!');
        
    } catch (error) {
        console.error('\n❌ gRPC test failed:', error.message);
        process.exit(1);
    }
}

testGrpcConnection();