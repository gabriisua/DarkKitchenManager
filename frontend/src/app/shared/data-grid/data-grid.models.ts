// ── Grid Type System ──
// Column config for typed grid rendering. T is the entity type.
// keyof T ensures field references are type-checked at compile time — no `any` allowed.
// cellType drives default rendering; customTemplateRef is the escape hatch for special columns.

export type CellType = 'text' | 'chip' | 'date' | 'currency' | 'boolean' | 'actions';

export interface ColumnDef<T> {
  field: keyof T & string;        // Must be a real property of T — never `string` generically
  header: string;                 // Display label in column header
  cellType?: CellType;            // Default: 'text'
  sortable?: boolean;             // Default: true
  width?: string;                 // CSS width (e.g. '120px')
  sticky?: 'start' | 'end';      // Sticky position (Material supports sticky cols)
  hidden?: boolean;               // For future column chooser
  customTemplateRef?: string;     // Reference name for ngTemplateOutlet
  divisor?: number;               // Divide cell value by this before display (e.g., 100 for cents → EUR)
}

// Common query params for all paginated endpoints — camelCase here,
// converted to PascalCase by buildPagedParams() for the API.
export interface PagedRequest {
  page: number;
  pageSize: number;
  search?: string;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc' | '';
  dateFrom?: string;
  dateTo?: string;
}

// Grid state emitted upward on changes
export interface SortChange {
  column: string;
  direction: 'asc' | 'desc' | '';
}

export interface PageChange {
  page: number;      // 0-based from MatPaginator
  pageSize: number;
}

// Entity-specific action definition for the actions column
export interface GridAction<T> {
  icon: string;
  label: string;
  color?: string;
  handler: (row: T) => void;
}

// Full grid configuration passed to DataGridComponent inputs
export interface GridConfig {
  pageSizeOptions?: number[];
  defaultPageSize?: number;
  hidePageSize?: boolean;
  showFirstLastButtons?: boolean;
}
