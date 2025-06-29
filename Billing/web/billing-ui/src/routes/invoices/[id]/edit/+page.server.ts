import { invoiceGrpcService } from "$lib/server/grpc-client";
import { error } from "@sveltejs/kit";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ params }) => {
    try {
        const invoice = await invoiceGrpcService.getInvoice(params.id);

        return {
            invoice,
        };
    } catch (err: any) {
        console.error("Failed to load invoice:", err);

        if (err.code === 5) {
            // NOT_FOUND
            throw error(404, "Invoice not found");
        }

        throw error(500, "Failed to load invoice");
    }
};
