/**
 * Format date for display in various formats
 */
export function formatDate(dateString: string | Date, format: 'short' | 'medium' | 'long' | 'relative' = 'medium'): string {
	const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
	
	if (isNaN(date.getTime())) {
		return 'Invalid date';
	}

	switch (format) {
		case 'short':
			return date.toLocaleDateString('en-US', {
				month: 'short',
				day: 'numeric',
				year: 'numeric'
			});
		
		case 'medium':
			return date.toLocaleDateString('en-US', {
				weekday: 'short',
				month: 'short', 
				day: 'numeric',
				year: 'numeric'
			});
		
		case 'long':
			return date.toLocaleDateString('en-US', {
				weekday: 'long',
				month: 'long',
				day: 'numeric', 
				year: 'numeric',
				hour: '2-digit',
				minute: '2-digit'
			});
		
		case 'relative':
			return formatRelativeDate(date);
		
		default:
			return date.toLocaleDateString();
	}
}

/**
 * Format relative time (e.g., "2 days ago", "in 3 hours")
 */
export function formatRelativeDate(date: Date): string {
	const now = new Date();
	const diffMs = now.getTime() - date.getTime();
	const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
	const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
	const diffMinutes = Math.floor(diffMs / (1000 * 60));

	if (diffDays > 0) {
		return diffDays === 1 ? '1 day ago' : `${diffDays} days ago`;
	} else if (diffDays < 0) {
		const futureDays = Math.abs(diffDays);
		return futureDays === 1 ? 'in 1 day' : `in ${futureDays} days`;
	} else if (diffHours > 0) {
		return diffHours === 1 ? '1 hour ago' : `${diffHours} hours ago`;
	} else if (diffHours < 0) {
		const futureHours = Math.abs(diffHours);
		return futureHours === 1 ? 'in 1 hour' : `in ${futureHours} hours`;
	} else if (diffMinutes > 0) {
		return diffMinutes === 1 ? '1 minute ago' : `${diffMinutes} minutes ago`;
	} else if (diffMinutes < 0) {
		const futureMinutes = Math.abs(diffMinutes);
		return futureMinutes === 1 ? 'in 1 minute' : `in ${futureMinutes} minutes`;
	} else {
		return 'just now';
	}
}

/**
 * Check if a date is overdue (past current date)
 */
export function isOverdue(dueDateString?: string): boolean {
	if (!dueDateString) return false;
	
	const dueDate = new Date(dueDateString);
	const now = new Date();
	
	// Set time to start of day for fair comparison
	dueDate.setHours(23, 59, 59, 999);
	now.setHours(0, 0, 0, 0);
	
	return dueDate < now;
}

/**
 * Format date for input fields (YYYY-MM-DD format)
 */
export function formatDateForInput(date: Date = new Date()): string {
	return date.toISOString().split('T')[0];
}

/**
 * Parse date from input field
 */
export function parseDateFromInput(dateString: string): Date | null {
	if (!dateString) return null;
	
	const date = new Date(dateString + 'T00:00:00');
	return isNaN(date.getTime()) ? null : date;
}