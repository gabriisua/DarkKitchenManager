import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, switchMap, tap } from 'rxjs';

import { StaffService } from '../../../core/services/staff.service';
import { UiService } from '../../../core/services/ui.service'; // <-- Iniezione UiService
import { Staff } from '../../../shared/models/api.models';
import { StaffDialogComponent } from './staff-dialog/staff-dialog.component';

import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import { ColumnDef, SortChange, PageChange } from '../../../shared/data-grid/data-grid.models';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';

// Usa la tua interfaccia di richiesta standardizzata
import { StaffPagedRequest } from '../../../shared/models/api.models';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatFormFieldModule, MatInputModule,
    MatIconModule, MatButtonModule, MatDialogModule, DataGridComponent, MatTooltip
  ],
  templateUrl: './staff-page.component.html',
  styleUrl: './staff-page.component.css'
})
export class StaffPageComponent implements OnInit {
  private staffService = inject(StaffService);
  private dialog = inject(MatDialog);
  private uiService = inject(UiService); // <-- Iniezione UiService

  readonly data = signal<Staff[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  searchTerm: string = '';

  // Tipizzazione stretta: solo le chiavi reali di 'Staff'
  columns: ColumnDef<Staff>[] = [
    { field: 'username', header: 'Username' },
    { field: 'email', header: 'Email' },
    { field: 'role', header: 'Ruolo' },
    { field: 'isActive', header: 'Stato', cellType: 'boolean' },
    { field: 'lastLogin', header: 'Ultimo Accesso', cellType: 'date' },
    //{ field: 'actions' as any, header: 'Azioni', sortable: false, cellType: 'actions' }
  ];

  public querySubject = new BehaviorSubject<StaffPagedRequest>({
    page: 1, // Partiamo da 1 come per Customer/Allergen se il backend lo richiede
    pageSize: 10
  });

  ngOnInit(): void {
    this.querySubject.pipe(
      tap(() => {
        this.loading.set(true);
        this.error.set(null);
      }),
      switchMap(query => this.staffService.getStaff(query))
    ).subscribe({
      next: (res: any) => {
        this.loading.set(false);

        if (res.succeeded && res.data && res.data.items) {
          this.data.set(res.data.items);
          this.totalItems.set(res.data.totalCount);
        } else if (res.items) {
          this.data.set(res.items);
          this.totalItems.set(res.totalCount);
        } else {
          this.data.set([]);
          this.totalItems.set(0);
          this.error.set(res.message || 'Nessun dato trovato o formato risposta non valido.');
        }
      },
      error: (err) => {
        console.error('Errore caricamento staff:', err);
        const errorMsg = 'Si è verificato un errore durante il caricamento dei dati.';
        this.loading.set(false);
        this.error.set(errorMsg);
        this.data.set([]);
        this.totalItems.set(0);

        // <-- Toast per errore caricamento
        this.uiService.showToast(errorMsg, 'error');
      }
    });
  }

  updateQuery(partialQuery: Partial<StaffPagedRequest>): void {
    const current = this.querySubject.value;
    const isNavigation = 'page' in partialQuery || 'pageSize' in partialQuery;

    this.querySubject.next({
      ...current,
      ...partialQuery,
      // Se è paginazione teniamo la page corrente, altrimenti per ricerche/ordinamenti resettiamo a 1
      page: isNavigation ? (partialQuery.page ?? current.page) : 1
    });
  }

  onSearch(value: string): void { this.updateQuery({ search: value }); }

  hasActiveFilters(): boolean {
    return !!this.searchTerm;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.updateQuery({ search: '', page: 1 });
  }

  onSortChange(sort: SortChange): void { this.updateQuery({ sortColumn: sort.column, sortDirection: sort.direction }); }
  onPageChange(event: PageChange): void { this.updateQuery({ page: event.page, pageSize: event.pageSize }); }
  refreshTable(): void { this.updateQuery({}); }

  addStaff(): void {
    const dialogRef = this.dialog.open(StaffDialogComponent, { width: '400px', disableClose: true, data: null });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.uiService.showToast('Utente creato con successo!');
        this.refreshTable();
      }
    });
  }

  editStaff(row: Staff): void {
    const dialogRef = this.dialog.open(StaffDialogComponent, { width: '400px', disableClose: true, data: row });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.uiService.showToast('Utente modificato con successo!');
        this.refreshTable();
      }
    });
  }

  deleteStaff(row: Staff): void {
    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare/disabilitare l'utente ${row.username}?`,
      () => {
        this.uiService.showLoader('Eliminazione in corso...');

        this.staffService.deleteStaff(row.id).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Utente eliminato correttamente.');
            this.refreshTable();
          },
          error: (err) => {
            console.error('Errore eliminazione staff:', err);
            this.uiService.hideLoader();
            this.uiService.showToast(err.error?.message || 'Errore durante l\'eliminazione dell\'utente.', 'error');
          }
        });
      },
      'Conferma Eliminazione'
    );
  }
}
