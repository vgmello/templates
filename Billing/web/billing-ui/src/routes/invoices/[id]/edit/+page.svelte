<script lang="ts">
import type { ActionData, PageData } from './$types';

let { data, form }: { data: PageData; form: ActionData } = $props();

</script>

<svelte:head>
	<title>Edit {data.invoice.name} - Invoice</title>
</svelte:head>

<div class="container mx-auto px-4 py-8 max-w-4xl">
    <p>Coming soon...</p>
</div>
