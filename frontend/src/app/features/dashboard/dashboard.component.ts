import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule],
  template: `
    <div class="dashboard">
      <h1>Dashboard</h1>
      <div class="cards">
        <mat-card class="dash-card">
          <mat-card-header>
            <mat-card-title>Benvenuto nel Back Office</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Seleziona una sezione dal menu laterale per iniziare.</p>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard { padding: 24px; }
    .dashboard h1 { font-size: 24px; font-weight: 500; margin-bottom: 20px; }
    .cards { display: grid; gap: 16px; }
  `]
})
export class DashboardComponent {}
