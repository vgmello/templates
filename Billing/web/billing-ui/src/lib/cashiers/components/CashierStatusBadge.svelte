<script lang="ts">
	import { Badge } from '$lib/ui/badge';

	type Props = {
		size?: 'sm' | 'default';
	};

	let { size = 'default' }: Props = $props();
</script>

<Badge variant="default" class={size === 'sm' ? 'text-xs' : ''} title="This cashier is active">
	Active
</Badge>
