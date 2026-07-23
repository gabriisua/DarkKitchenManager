import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SelectionModel } from '@angular/cdk/collections';
import { MenuService } from '../../../../core/services/menu.service';
import { UiService } from '../../../../core/services/ui.service';
import { PlateService } from '../../../../core/services/plate.service';
import { MenuDetail, PrintBatchItem } from '../../../../shared/models/api.models';
import { PrintService } from '../../../../core/services/print.service';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-menu-detail',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatTooltipModule, MatTableModule, MatDividerModule,
    MatCheckboxModule, MatFormFieldModule, MatInputModule,
    MatMenuModule, MatButtonToggleModule, MatDatepickerModule, MatNativeDateModule
  ],
  templateUrl: './menu-detail.component.html',
  styleUrls: ['./menu-detail.component.scss']
})
export class MenuDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private menuService = inject(MenuService);
  private uiService = inject(UiService);
  private plateService = inject(PlateService);
  private printService = inject(PrintService);

  menu = signal<MenuDetail | null>(null);
  loading = signal(true);
  selectedPrintFormat = signal<'standard' | 'cortilia' | 'foorban' | 'crio'>('standard');

  isCrioThawed = false;
  crioProductionDate: Date = new Date();
  crioThawingDate: Date = new Date();
  targetLanguage: string = 'IT';

  get predictedCrioExpiryDate(): Date {
    const d = new Date();
    if (this.isCrioThawed) {
      const base = new Date(this.crioThawingDate);
      base.setDate(base.getDate() + 15);
      return base;
    }
    const base = new Date(this.crioProductionDate);
    base.setMonth(base.getMonth() + 7);
    return base;
  }

  selection = new SelectionModel<any>(true, []);

  displayedColumns = computed(() => {
    const baseCols = ['select', 'plateName', 'weight', 'overridePrice', 'customExpiryDate'];

    // Aggiungi colonne Foorban se il formato è quello
    if (this.selectedPrintFormat() === 'foorban') {
      baseCols.push('isWow', 'isXl');
    }

    baseCols.push('copies', 'pauseAfter', 'lotNumber', 'actions');
    return baseCols;
  });

  groupedMenuItems = computed(() => {
    const currentMenu = this.menu();
    if (!currentMenu?.menuItems?.length) return [];

    const groups = new Map<string, any[]>();

    for (const item of currentMenu.menuItems) {
      const expiryDate = new Date();
      const shelfLifeDays = item.daysToExpire ?? 3;
      expiryDate.setDate(expiryDate.getDate() + shelfLifeDays);

      const pad = (n: number) => n < 10 ? '0' + n : n;
      const calculatedDateStr = `${expiryDate.getFullYear()}-${pad(expiryDate.getMonth() + 1)}-${pad(expiryDate.getDate())}`;

      if (!(item as any)._print) {
        (item as any)._print = {
          copies: 1,
          pauseAfter: 0,
          lotNumber: '',
          customExpiryDate: calculatedDateStr,
          customWeight: item.plateWeight || 0,
          isWow: false,
          isXl: false
        };
      }

      const cat = item.categoryName || 'Varie';
      if (!groups.has(cat)) {
        groups.set(cat, []);
      }
      groups.get(cat)!.push(item);
    }

    return Array.from(groups.entries()).map(([category, items]) => ({
      category,
      items
    }));
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.uiService.showToast('ID Menu non valido', 'error');
      this.router.navigate(['/menus']);
      return;
    }
    this.loadMenu(id);
  }

  private loadMenu(id: number): void {
    this.uiService.showLoader('Caricamento menu...');
    this.loading.set(true);

    this.menuService.getById(id).subscribe({
      next: (res) => {
        this.menu.set((res as any).data || res);
        this.loading.set(false);
        this.uiService.hideLoader();
      },
      error: () => {
        this.uiService.hideLoader();
        this.loading.set(false);
        this.uiService.showToast('Errore nel caricamento del menu', 'error');
        this.router.navigate(['/menus']);
      }
    });
  }

  downloadMenuPdf(): void {
    const currentMenu = this.menu();
    if (!currentMenu) return;

    this.uiService.showLoader('Generazione PDF menu...');
    this.menuService.downloadMenuPdf(currentMenu.id).subscribe({
      next: (blob) => {
        this.uiService.hideLoader();
        const safeName = currentMenu.name.replace(/\s+/g, '_');
        const fileName = `${safeName}_Menu.pdf`;
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.uiService.hideLoader();
        this.uiService.showToast('Errore durante il download del PDF', 'error');
      }
    });
  }

  private resetPrintForm(item: any): void {
    const expiryDate = new Date();
    const shelfLifeDays = item.daysToExpire ?? 3;
    expiryDate.setDate(expiryDate.getDate() + shelfLifeDays);
    const pad = (n: number) => n < 10 ? '0' + n : n;
    const calculatedDateStr = `${expiryDate.getFullYear()}-${pad(expiryDate.getMonth() + 1)}-${pad(expiryDate.getDate())}`;

    item._print = {
      copies: 1,
      pauseAfter: 0,
      lotNumber: '',
      customExpiryDate: calculatedDateStr,
      customWeight: item.plateWeight || 0,
      isWow: false,
      isXl: false
    };
  }

  toggleWow(item: any, checked: boolean) {
    item._print.isWow = checked;
    if (checked) item._print.isXl = false;
  }

  toggleXl(item: any, checked: boolean) {
    item._print.isXl = checked;
    if (checked) item._print.isWow = false;
  }

  printSingle(item: any): void {
    const format = this.selectedPrintFormat();

    if (!item._print.customExpiryDate) {
      this.uiService.showToast('Impossibile stampare: inserire una data di scadenza.', 'error');
      return;
    }

    if (!item._print.lotNumber || !item._print.lotNumber.trim()) {
      this.uiService.showToast('Impossibile stampare: inserire il numero di lotto.', 'error');
      return;
    }

    let expiryDate = item._print.customExpiryDate;
    if (format === 'crio') {
      const pad = (n: number) => n < 10 ? '0' + n : n;
      if (this.isCrioThawed) {
        const d = new Date(this.crioThawingDate);
        d.setDate(d.getDate() + 15);
        expiryDate = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
      } else {
        const d = new Date(this.crioProductionDate);
        d.setMonth(d.getMonth() + 7);
        expiryDate = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
      }
    }

    const request: any = {
      copies: item._print.copies,
      pauseAfter: item._print.pauseAfter,
      lotNumber: item._print.lotNumber.trim(),
      customExpiryDate: expiryDate,
      customWeight: item._print.customWeight || null,
      isWow: item._print.isWow,
      isXl: item._print.isXl
    };

    if (format === 'crio') {
      request.isThawed = this.isCrioThawed;
      request.thawingDate = this.crioThawingDate;
      request.targetLanguage = this.targetLanguage;
    }

    if (request.copies < 1) {
      this.uiService.showToast('Inserire almeno 1 copia', 'error');
      return;
    }

    this.uiService.askConfirm(
      `Stai per stampare in formato ${format.toUpperCase()}.\n\nHai effettuato la calibrazione della macchina e verificato che il rotolo inserito sia quello corretto?`,
      () => {
        this.uiService.showLoader(`Invio stampa ZPL (${format})...`);

        let request$;
        switch (format) {
          case 'standard':
            request$ = this.printService.printSingleLabel(item.plateId, request);
            break;
          case 'cortilia':
            request$ = this.printService.printCortiliaSingleLabel(item.plateId, request);
            break;
          case 'foorban':
            request$ = this.printService.printFoorbanSingleLabel(item.plateId, request);
            break;
          case 'crio':
            request$ = this.printService.printCrioSingleLabel(item.plateId, request);
            break;
        }

        request$.subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.resetPrintForm(item);
            this.uiService.showToast('Stampa inviata con successo', 'success');
          },
          error: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Errore durante la stampa', 'error');
          }
        });
      },
      'Verifica Stampante'
    );
  }

  printBatch(): void {
    const format = this.selectedPrintFormat();
    const selected = this.selection.selected;
    if (!selected.length) return;

    const hasMissingDates = selected.some((item: any) => !item._print.customExpiryDate);
    if (hasMissingDates) {
      this.uiService.showToast('Impossibile procedere: tutti i piatti selezionati devono avere una data di scadenza.', 'error');
      return;
    }

    const hasMissingLots = selected.some((item: any) => !item._print.lotNumber || !item._print.lotNumber.trim());
    if (hasMissingLots) {
      this.uiService.showToast('Impossibile procedere: tutti i piatti selezionati devono avere il numero di lotto.', 'error');
      return;
    }

    const batchItems: PrintBatchItem[] = selected.map((item: any) => {
      let expiryDate = item._print.customExpiryDate;
      if (format === 'crio') {
        const pad = (n: number) => n < 10 ? '0' + n : n;
        if (this.isCrioThawed) {
          const d = new Date(this.crioThawingDate);
          d.setDate(d.getDate() + 15);
          expiryDate = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
        } else {
          const d = new Date(this.crioProductionDate);
          d.setMonth(d.getMonth() + 7);
          expiryDate = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
        }
      }

      const batchItem: any = {
        plateId: item.plateId,
        copies: item._print.copies,
        pauseAfter: item._print.pauseAfter,
        lotNumber: item._print.lotNumber.trim(),
        customExpiryDate: expiryDate,
        customWeight: item._print.customWeight || null,
        isWow: item._print.isWow,
        isXl: item._print.isXl
      };

      if (format === 'crio') {
        batchItem.isThawed = this.isCrioThawed;
        batchItem.thawingDate = this.crioThawingDate;
        batchItem.targetLanguage = this.targetLanguage;
      }

      return batchItem;
    });

    this.uiService.askConfirm(
      `Stai per stampare un lotto in formato ${format.toUpperCase()}.\n\nHai effettuato la calibrazione della macchina e verificato che il rotolo inserito sia quello corretto?`,
      () => {
        this.uiService.showLoader(`Invio Batch ZPL (${format})...`);

        let request$;
        switch (format) {
          case 'standard':
            request$ = this.printService.printBatchLabels(batchItems);
            break;
          case 'cortilia':
            request$ = this.printService.printCortiliaBatchLabels(batchItems);
            break;
          case 'foorban':
            request$ = this.printService.printFoorbanBatchLabels(batchItems);
            break;
          case 'crio':
            request$ = this.printService.printCrioBatchLabels(batchItems);
            break;
        }

        request$.subscribe({
          next: () => {
            this.uiService.hideLoader();
            selected.forEach((item: any) => this.resetPrintForm(item));
            this.selection.clear();
            this.uiService.showToast('Batch inviato con successo', 'success');
          },
          error: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Errore durante l\'invio del batch', 'error');
          }
        });
      },
      'Verifica Stampante'
    );
  }

  isAllSelected(): boolean {
    const items = this.groupedMenuItems().flatMap(g => g.items);
    return items.length > 0 && items.every(item => this.selection.isSelected(item));
  }

  isPartiallySelected(): boolean {
    const items = this.groupedMenuItems().flatMap(g => g.items);
    return items.some(item => this.selection.isSelected(item)) && !this.isAllSelected();
  }

  toggleAllSelection(): void {
    const items = this.groupedMenuItems().flatMap(g => g.items);
    if (this.isAllSelected()) {
      this.selection.clear();
    } else {
      this.selection.select(...items);
    }
  }

  goBack(): void {
    this.router.navigate(['/menus']);
  }
}
