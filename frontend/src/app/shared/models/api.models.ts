// Auth

export interface AuthResponse {
  token: string;
  user?: Staff;
  message?: string;
}
export interface LoginRequest {
  email: string;
  password: string;
}

// Staff

export interface Staff {
  id: string;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
  lastLogin: string;
}
export interface StaffCreateRequest {
  username: string;
  email: string;
  password?: string;
  role: string;
}

export interface StaffPagedResponse {
  items: Staff[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface StaffFilter {
  page: number;
  pageSize: number;
  search?: string;
  email?: string;
  role?: string;
  sortColumn?: string;
  sortDirection?: string;
  dateFrom?: string;
  dateTo?: string;
}
export interface StaffUpdateRequest {
  email: string;
  password?: string;
  role: string;
}

// Allergen

export interface Allergen {
  id: number;
  name: string;
  code: string;
  description: string;
}

// Ingredient

export interface Ingredient {
  id: number;
  name: string;
  energyKjPer100g: number;
  energyKcalPer100g: number;
  fatsPer100g: number;
  saturatedFatsPer100g: number;
  carbohydratesPer100g: number;
  sugarsPer100g: number;
  fibersPer100g: number;
  proteinsPer100g: number;
  saltPer100g: number;
  costPer1000g: number;
  yieldPercentage: number;
  allergenIds: number[];
}

export interface IngredientCreateRequest {
  name: string;
  energyKjPer100g: number;
  energyKcalPer100g: number;
  fatsPer100g: number;
  saturatedFatsPer100g: number;
  carbohydratesPer100g: number;
  sugarsPer100g: number;
  fibersPer100g: number;
  proteinsPer100g: number;
  saltPer100g: number;
  costPer1000g: number;
  yieldPercentage: number;
  allergenIds: number[];
}

// ==========================================
// --- CATALOGO: CATEGORIE ---
// ==========================================

export interface Category {
  id: number;
  name: string;
  isActive: boolean;
}

export interface CategoryCreateRequest {
  name: string;
}

export interface CategoryUpdateRequest {
  name: string;
  isActive: boolean;
}

// Plate

export interface PlateIngredientInput {
  ingredientId: number;
  weightInGrams: number;
}

export interface PlateCreateRequest {
  name: string;
  description: string;
  categoryId: number;
  basePrice: number;
  packagingCost: number;
  ingredients: PlateIngredientInput[];
  lineType?: number;
  dietaryIcon?: number;
  isWowPlate?: boolean;
  isXlPlate?: boolean;
}

export interface Plate {
  id: number;
  name: string;
  description: string;
  categoryId: number;
  categoryName?: string;
  basePrice: number;
  packagingCost: number;
  lineType?: number;
  dietaryIcon?: number;
  isWowPlate?: boolean;
  isXlPlate?: boolean;
}

export interface FoodCost {
  plateId: number;
  foodCostCents: number;
  formattedDisplay: string;
}

export interface NutritionInfo {
  totalWeightGrams: number;
  energyKcal: number;
  fats: number;
  saturatedFats: number;
  carbohydrates: number;
  sugars: number;
  fibers: number;
  proteins: number;
  salt: number;
  allergens: Allergen[];
}

// --- ZPL Printing ---

export interface PrintLabelRequest {
  copies: number;
  pauseAfter: number;
  lotNumber?: string | null;
  customExpiryDate?: string | null;
  customWeight?: number;
  isThawed?: boolean;
  thawingDate?: string | Date;
  targetLanguage?: string;
}

export interface PrintBatchItem {
  plateId: number;
  copies: number;
  pauseAfter: number;
  lotNumber?: string | null;
  customExpiryDate?: string | null;
  customWeight?: number;
  isThawed?: boolean;
  thawingDate?: string | Date;
  targetLanguage?: string;
}

// Customer

export interface Customer {
  id: string;
  email: string;
  type: string;
  shippingAddress: string;
  city: string;
  zipCode: string;
  contactPhone: string;
  businessName: string;
  vatNumber: string;
  fiscalCode: string;
  sdiCode: string;
  pec: string;
  deliveryOpenTime: any;
  deliveryCloseTime: any;
  isActive: boolean;
}

export interface CustomerFilter {
  page: number;
  pageSize: number;
  search?: string;
  type?: string;
  isActive?: boolean | null;
  sortColumn?: string;
  sortDirection?: string;
  dateFrom?: string;
  dateTo?: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ── Standardized Paged Request Types ──
// All extend the common PagedRequest base with entity-specific filter fields.

import type { PagedRequest } from '../data-grid/data-grid.models';

export interface StaffPagedRequest extends PagedRequest {
  email?: string;
  role?: string;
}

export interface CustomerPagedRequest extends PagedRequest {
  type?: string;
  isActive?: boolean | null;    // tri-state: true/false/omit when null
}

export interface IngredientPagedRequest extends PagedRequest {
  name?: string;
  minEnergyKcal?: number;
  maxEnergyKcal?: number;
  minCost?: number;
  maxCost?: number;
  isActive?: boolean;
}

export interface PlatePagedRequest extends PagedRequest {
  name?: string;
  categoryId?: number;
  isActive?: boolean;
  minPrice?: number;
  maxPrice?: number;
  lineType?: number;
  dietaryIcon?: number;
  isWowPlate?: boolean;
  isXlPlate?: boolean;
}

export interface CategoryPagedRequest extends PagedRequest {
  name?: string;
  isActive?: boolean | null;
}

export interface AllergenPagedRequest extends PagedRequest {
  name?: string;
  code?: string;
}

// ==========================================
// --- MENU ---
// ==========================================

export interface Menu {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  customerId?: string | null;  // Null = global menu
}

export interface MenuPagedRequest extends PagedRequest {
  name?: string;
  isActive?: boolean | null;
}

export interface MenuItemDto {
  menuId: number;
  plateId: number;
  plateName: string;
  plateWeight: number;
  daysToExpire: number;
  basePrice: number;
  categoryName: string;
  overridePrice: number | null;
  availableFrom: string | null;
  availableTo: string | null;
  availableDaysOfWeek?: string | null;
}

export interface MenuDetail extends Menu {
  menuItems: MenuItemDto[];
}

export interface MenuCreateRequest {
  name: string;
  description: string;
  isActive: boolean;
  customerId?: string | null;
  menuItems: {
    plateId: number;
    overridePrice?: number | null;
    availableFrom?: string | null;
    availableTo?: string | null;
    availableDaysOfWeek?: string | null;
  }[];
}

// ==========================================
// --- SALES & DISCOUNTS MODELS ---
// ==========================================

// ── Paginazione Globale ──
export interface DiscountPagedRequest extends PagedRequest {
  isActive?: boolean | null;
}

// ── DTOs Griglie Globali (Sola Lettura) ──
export interface PlateDiscountDto {
  customerId: string;
  businessName: string;
  plateId: number;
  plateName: string;
  overridePrice: number; // In centesimi
  validFrom?: string | null;
  validTo?: string | null;
  isActive: boolean;
}

export interface CategoryDiscountDto {
  customerId: string;
  businessName: string;
  categoryId: number;
  categoryName: string;
  discountPercentage: number;
  validFrom?: string | null;
  validTo?: string | null;
  isActive: boolean;
}

// ── DTOs Singolo Cliente (Esistenti) ──
export interface ClientCategoryDiscount {
  customerId: string;
  categoryId: number;
  discountPercentage: number;
  validFrom?: string | null;
  validTo?: string | null;
  category?: any;
}

export interface ClientPlateDiscount {
  customerId: string;
  plateId: number;
  overridePrice: number; // In centesimi
  validFrom?: string | null;
  validTo?: string | null;
  plate?: any;
}

// ── Payloads Form (Creazione / Upsert) ──
export interface SetCategoryDiscountPayload {
  categoryId: number;
  discountPercentage: number;
  validFrom?: string | null;
  validTo?: string | null;
}

export interface SetPlateDiscountPayload {
  plateId: number;
  overridePrice: number; // Da inviare in centesimi!
  validFrom?: string | null;
  validTo?: string | null;
}


//ORDINI

export enum OrderStatus {
  Pending = 1,
  Confirmed = 2,
  InProduction = 3,
  Shipped = 4,
  Delivered = 5,
  Cancelled = 6
}

export interface OrderItemResponseDto {
  id: number;
  plateId: number;
  plateNameSnapshot: string;
  quantity: number;
  unitPriceNetCents: number;
  vatRate: number;
  appliedDiscountNote?: string;
}

export interface OrderResponseDto {
  id: string;
  customerId: string;
  deliveryHubId: string;
  customerBusinessName: string;
  deliveryHubName: string;
  orderNumber: string;
  customerReference?: string;
  orderDate: string;
  requestedDeliveryDate: string;
  calculatedDeliveryDate: string;
  status: OrderStatus;
  netAmountCents: number;
  vatAmountCents: number;
  totalGrossCents: number;
  shippingCostCents: number;
  deliveryNotes?: string;
  invoiceNumber?: string;
  items: OrderItemResponseDto[];
}
// ── Invoice: Pending Summary ──

export interface PendingInvoiceSummary {
  customerId: string;
  businessName: string;
  vatNumber: string;
  ordersCount: number;         // AGGIORNATO
  netAmountCents: number;      // AGGIORNATO
  vatAmountCents: number;      // AGGIORNATO
  totalGrossCents: number;     // AGGIORNATO
  hasFailedInvoices: boolean;  // NUOVO
}

export interface PendingOrderItem {
  orderId: string;
  orderNumber: string;
  orderDate: string;
  calculatedDeliveryDate: string;
  totalGrossCents: number;
  customerReference?: string;
  latestErrorMessage?: string; // NUOVO
}

export interface BulkInvoiceRequest {
  orderIds: string[];
  sendToSdiImmediately: boolean;
}

export interface InvoicePendingSummaryRequest {
  page: number;
  pageSize: number;
  search?: string;
  dateFrom?: string;
  dateTo?: string;
}

// ── Invoice History ──

export interface InvoiceHistoryItem {
  ficDocumentId: number;
  invoiceNumber: string;
  customerName: string;
  ordersCount: number;
  totalGrossCents: number;
  maxDeliveryDate: string;  // ISO date string
}

export interface InvoiceHistoryRequest extends PagedRequest {
  dateFrom?: string;
  dateTo?: string;
}
