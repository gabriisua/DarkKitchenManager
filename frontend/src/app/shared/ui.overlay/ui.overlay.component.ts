import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon'; // <-- Aggiunto per l'icona di warning
import { UiService } from '../../core/services/ui.service';

@Component({
  selector: 'app-ui-overlay',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule], // <-- Aggiunto qui
  templateUrl: './ui-overlay.component.html',
  styleUrls: ['./ui-overlay.component.css']
})
export class UiOverlayComponent {
  ui = inject(UiService);
}
