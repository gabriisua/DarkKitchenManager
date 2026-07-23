import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error';

interface ToastState {
  visible: boolean;
  type: ToastType;
  message: string;
}

interface ConfirmState {
  visible: boolean;
  title: string;
  message: string;
  onConfirm?: () => void;
}

interface AlertState {
  visible: boolean;
  title: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class UiService {

  // =========================
  // Loader
  // =========================

  busy = signal(false);
  busyMessage = signal('Caricamento...');

  showLoader(message = 'Caricamento...') {
    this.busyMessage.set(message);
    this.busy.set(true);
  }

  hideLoader() {
    this.busy.set(false);
  }

  // =========================
  // Toast
  // =========================

  toast = signal<ToastState>({
    visible: false,
    type: 'success',
    message: ''
  });

  showToast(
    message: string,
    type: ToastType = 'success',
    duration = 2500
  ) {

    this.toast.set({
      visible: true,
      message,
      type
    });

    setTimeout(() => {
      this.toast.set({
        visible: false,
        message: '',
        type
      });
    }, duration);
  }

  // =========================
  // Confirm Dialog
  // =========================

  confirm = signal<ConfirmState>({
    visible: false,
    title: '',
    message: ''
  });

  askConfirm(
    message: string,
    onConfirm: () => void,
    title = 'Conferma eliminazione'
  ) {

    this.confirm.set({
      visible: true,
      title,
      message,
      onConfirm
    });
  }

  confirmYes() {

    const action = this.confirm().onConfirm;

    this.confirm.set({
      visible: false,
      title: '',
      message: ''
    });

    action?.();
  }

  confirmNo() {
    this.confirm.set({
      visible: false,
      title: '',
      message: ''
    });
  }

  // =========================
  // Alert Dialog (Errore Bloccante)
  // =========================

  alertState = signal<AlertState>({
    visible: false,
    title: '',
    message: ''
  });

  showAlert(title: string, message: string) {
    this.alertState.set({
      visible: true,
      title,
      message
    });
  }

  closeAlert() {
    this.alertState.set({
      visible: false,
      title: '',
      message: ''
    });
  }
}
