import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export enum ToastType {
  Success = 'success',
  Error = 'error',
  Warning = 'warning',
  Info = 'info'
}

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastSubject = new BehaviorSubject<Toast[]>([]);
  public toasts$ = this.toastSubject.asObservable();

  private defaultDuration = 5000;

  constructor() {}

  show(message: string, type: ToastType = ToastType.Info, duration?: number): void {
    const id = this.generateId();
    const toast: Toast = {
      id,
      message,
      type,
      duration: duration || this.defaultDuration
    };

    const currentToasts = this.toastSubject.value;
    this.toastSubject.next([...currentToasts, toast]);

    if (toast.duration && toast.duration > 0) {
      setTimeout(() => this.remove(id), toast.duration);
    }
  }

  success(message: string, duration?: number): void {
    this.show(message, ToastType.Success, duration);
  }

  error(message: string, duration?: number): void {
    this.show(message, ToastType.Error, duration || 7000);
  }

  warning(message: string, duration?: number): void {
    this.show(message, ToastType.Warning, duration);
  }

  info(message: string, duration?: number): void {
    this.show(message, ToastType.Info, duration);
  }

  remove(id: string): void {
    const toasts = this.toastSubject.value.filter(t => t.id !== id);
    this.toastSubject.next(toasts);
  }

  clear(): void {
    this.toastSubject.next([]);
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}
