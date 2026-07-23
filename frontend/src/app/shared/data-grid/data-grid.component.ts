import { Component, Input, Output, EventEmitter, ContentChild, TemplateRef, ChangeDetectionStrategy, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ColumnDef, CellType, SortChange, PageChange, GridConfig } from './data-grid.models';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatSortModule, MatPaginatorModule,
    MatProgressSpinnerModule, MatButtonModule, MatIconModule, MatChipsModule
  ],
  templateUrl: './data-grid.component.html',
  styleUrl: './data-grid.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DataGridComponent<T extends Record<string, any>> {
  // ── Required Inputs ──
  /** Column definitions — typed via ColumnDef<T>.field: keyof T */
  @Input({ required: true }) columns!: ColumnDef<T>[];
  /** Data rows to render */
  @Input({ required: true }) data: T[] = [];
  /** Total record count (for paginator) */
  @Input({ required: true }) totalItems: number = 0;
  /** Loading state — shows spinner overlay when true */
  @Input() loading: boolean = false;
  /** Error message — shows error banner when non-null. null = no error */
  @Input() error: string | null = null;

  // ── Optional Inputs ──
  @Input() config: GridConfig = {};

  // ── Outputs ──
  @Output() sortChange = new EventEmitter<SortChange>();
  @Output() pageChange = new EventEmitter<PageChange>();
  @Output() retry = new EventEmitter<void>();

  // ── Content Projection ──
  /** Template for per-row action buttons projected from smart component */
  @ContentChild('actionsTemplate', { static: false }) actionsTemplateRef?: TemplateRef<any>;
  /** Column def key for the actions column */
  @Input() actionsColumnDef: string = 'actions';

  // ── Computed ──
  /** List of all displayed column keys (hidden columns excluded) */
  displayedColumns = computed(() =>
    this.columns.filter(c => !c.hidden).map(c => c.field as string)
  );

  /** All displayed columns + actions column when template is provided */
  allDisplayedColumns = computed(() => {
    const cols = this.columns.filter(c => !c.hidden).map(c => c.field as string);
    if (this.actionsTemplateRef) {
      return [...cols, this.actionsColumnDef];
    }
    return cols;
  });

  pageSizeOptions = computed(() => this.config.pageSizeOptions ?? [10, 25, 50, 100]);
  defaultPageSize = computed(() => this.config.defaultPageSize ?? 10);

  // ── Event Handlers ──
  onSortChange(sort: Sort): void {
    this.sortChange.emit({ column: sort.active, direction: sort.direction as 'asc' | 'desc' | '' });
  }

  onPageChange(event: PageEvent): void {
    this.pageChange.emit({ page: event.pageIndex, pageSize: event.pageSize });
  }

  trackByField(_index: number, col: ColumnDef<T>): string {
    return col.field as string;
  }

  /** Get cell display value (simple text rendering) */
  getCellValue(row: T, field: keyof T): string {
    const val = row[field];
    if (val === null || val === undefined) return '-';
    return String(val);
  }

  /** Get cell rendering type for a column */
  getCellType(col: ColumnDef<T>): CellType {
    return col.cellType ?? 'text';
  }

  /** Type guard for boolean values */
  isBoolean(val: any): boolean {
    return typeof val === 'boolean';
  }
}
