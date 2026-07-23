import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { PlateService } from '../../../../core/services/plate.service';
import { UiService } from '../../../../core/services/ui.service';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';

import { FoodCost, NutritionInfo } from '../../../../shared/models/api.models';

@Component({
  selector: 'app-plate-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CurrencyPipe,
    DecimalPipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule
  ],
  templateUrl: './plate-detail.component.html',
  styleUrls: ['./plate-detail.component.scss']
})
export class PlateDetailComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private plateService = inject(PlateService);
  private uiService = inject(UiService);

  plate = signal<any>(null);
  foodCost = signal<FoodCost | null>(null);
  nutrition = signal<NutritionInfo | null>(null);

  loading = signal(true);

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!id) {
      this.uiService.showToast('ID Piatto non valido', 'error');
      this.router.navigate(['/plates']);
      return;
    }

    this.plateService.getById(id).subscribe({
      next: (res: any) => {
        this.plate.set(res.data || res);
        this.loading.set(false);
      },
      error: () => {
        this.uiService.showToast('Impossibile caricare il piatto.', 'error');
        this.loading.set(false);
        this.router.navigate(['/plates']);
      }
    });

    this.plateService.getFoodCost(id).subscribe({
      next: (res: any) => this.foodCost.set(res.data || res),
      error: () => console.warn('Food cost non calcolabile per questo piatto.')
    });

    this.plateService.getNutrition(id).subscribe({
      next: (res: any) => this.nutrition.set(res.data || res),
      error: () => console.warn('Valori nutrizionali non calcolabili per questo piatto.')
    });
  }

  getFormattedBasePrice(): number {
    const currentPlate = this.plate();
    if (!currentPlate || currentPlate.basePrice == null) return 0;
    return currentPlate.basePrice / 100;
  }

  getFormattedPackagingCost(): number {
    const currentPlate = this.plate();
    if (!currentPlate || currentPlate.packagingCost == null) return 0;
    return currentPlate.packagingCost / 100;
  }

  getLineTypeName(value: number): string {
    const types = ['Standard', 'Gourmet', 'Vegetale', 'Fitness', 'Planted'];
    return types[value] || 'Standard';
  }

  getDietaryIconName(value: number): string {
    const icons = ['Nessuna', 'Vegano', 'Vegetariano', 'Carne', 'Pesce'];
    return icons[value] || 'Nessuna';
  }

  editPlate(): void {
    const currentPlate = this.plate();
    if (currentPlate && currentPlate.id) {
      this.router.navigate(['/plates', currentPlate.id, 'edit']);
    }
  }

  downloadTechnicalSheet(): void {
    const currentPlate = this.plate();
    if (!currentPlate || !currentPlate.id) return;

    this.uiService.showLoader('Generazione Scheda Tecnica in corso...');

    this.plateService.downloadTechnicalSheet(currentPlate.id).subscribe({
      next: (blob: Blob) => {
        this.uiService.hideLoader();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Scheda_Tecnica_${currentPlate.code || currentPlate.id}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.uiService.hideLoader();
        this.uiService.showToast('Errore durante la generazione della scheda tecnica', 'error');
      }
    });
  }
}
