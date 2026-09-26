// ─── Auth ────────────────────────────────────────────────────────────────────
// What GET /users/me returns. The token itself is never exposed to the browser.
export interface CurrentUser {
  id: string;
  email: string;
  roles: string[];
}

export type UserRole = 'Admin' | 'User';

export interface AuthUser {
  id: string;
  email: string;
  role: UserRole;
}

// ─── Products ─────────────────────────────────────────────────────────────────
export interface ProductAttribute {
  name: string;
  value: string;
}

export interface Image {
  id: string;
  url: string;
  fileName: string;
  uploadedAt: string;
  isPrimary: boolean;
}

export interface Product {
  id: string;
  name: string;
  price: number;
  category: ProductCategory;
  manufacturer?: string | null;
  weightGrams?: number | null;
  groupId?: string | null;
  variantName?: string | null;
  sourceUrl?: string | null;
  needsReview?: boolean;
  stockQuantity: number;
  attributes: ProductAttribute[];
  images?: Image[];
  spec?: ComponentSpec | null;
  group?: ProductGroup | null;
}

export interface ProductVariant {
  id: string;
  variantName?: string | null;
  price: number;
  stockQuantity: number;
}

export interface ProductGroup {
  id: string;
  name: string;
  variantCount: number;
  minPrice: number;
  maxPrice: number;
  totalStock: number;
  variants?: ProductVariant[] | null;
}

// ─── Component specs ──────────────────────────────────────────────────────────
export const COMPONENT_TYPES = [
  'Frame', 'Motor', 'Propeller', 'FlightController', 'Esc', 'Stack',
  'Battery', 'VideoTransmitter', 'Camera', 'Receiver', 'Antenna',
] as const;
export type ComponentType = typeof COMPONENT_TYPES[number];

export const OTHER_CATEGORIES = ['Goggles', 'Radio', 'Charger', 'Tool', 'ReadyToFly', 'Accessory'] as const;
export const PRODUCT_CATEGORIES = [...COMPONENT_TYPES, ...OTHER_CATEGORIES] as const;
export type ProductCategory = typeof PRODUCT_CATEGORIES[number];

export const MOUNT_PATTERNS = ['M9x9', 'M12x12', 'M16x16', 'M19x19', 'M20x20', 'M25_5x25_5', 'M30_5x30_5'] as const;
export type MountPattern = typeof MOUNT_PATTERNS[number];

export const VIDEO_SYSTEMS = ['Analog', 'DjiO3', 'DjiO4', 'Walksnail', 'HdZero'] as const;
export type VideoSystem = typeof VIDEO_SYSTEMS[number];

export const BATTERY_CONNECTORS = ['Xt30', 'Xt60', 'Xt90', 'Xt150', 'Ec5', 'Bt20', 'Ph20'] as const;
export type BatteryConnector = typeof BATTERY_CONNECTORS[number];

export const RF_CONNECTORS = ['Ufl', 'Mmcx', 'Sma', 'RpSma', 'Ipex4', 'Mcx'] as const;
export type RfConnector = typeof RF_CONNECTORS[number];

export const RADIO_PROTOCOLS = ['ExpressLrs', 'Crossfire', 'Tracer', 'Ghost', 'FrSky'] as const;
export type RadioProtocol = typeof RADIO_PROTOCOLS[number];

export type ComponentSpec =
  | { type: 'Frame'; maxPropSizeInch: number; fcMountPatterns: MountPattern[]; motorMountPatterns: MountPattern[]; cameraWidthMm: number | null }
  | { type: 'Motor'; statorSize: string; kv: number; mountPattern: MountPattern; minCells: number; maxCells: number; maxCurrentA: number | null; shaftMm: number | null; maxThrustGrams?: number | null }
  | { type: 'Propeller'; diameterInch: number; pitchInch: number | null; bladeCount: number | null; hubMm: number | null }
  | { type: 'FlightController'; mountPattern: MountPattern; minCells: number; maxCells: number }
  | { type: 'Esc'; mountPattern: MountPattern; minCells: number; maxCells: number; continuousCurrentA: number | null; batteryConnector: BatteryConnector | null }
  | { type: 'Battery'; cells: number; capacityMah: number; cRating: number | null; connector: BatteryConnector }
  | { type: 'VideoTransmitter'; videoSystem: VideoSystem; antennaConnector: RfConnector | null; mountPattern: MountPattern | null }
  | { type: 'Camera'; videoSystem: VideoSystem; widthMm: number | null }
  | { type: 'Receiver'; protocol: RadioProtocol }
  | { type: 'Antenna'; connector: RfConnector }
  | { type: 'Stack'; mountPattern: MountPattern; minCells: number; maxCells: number; continuousCurrentA: number | null; batteryConnector: BatteryConnector | null };

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApiError {
  code: string;
  field?: string | null;
  message: string;
}

export interface ApiPagination {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiResponse<T = undefined> {
  success: boolean;
  errors: ApiError[];
  data?: T;
  pagination?: ApiPagination | null;
}

export interface ProductFilters {
  page?: number;
  pageSize?: number;
  name?: string;
  minPrice?: number | '';
  maxPrice?: number | '';
  category?: ProductCategory;
}

// ─── Cart ─────────────────────────────────────────────────────────────────────
export interface CartItem {
  cartId: string;
  productId: string;
  productName: string;
  productImageUrl?: string;
  quantity: number;
  price: number;
}

export interface Cart {
  userId: string;
  cartItems: CartItem[];
  totalPrice: number;
  createdAt: string;
}

export interface CreateCartItem {
  productId: string;
  quantity: number;
}

// ─── Orders ───────────────────────────────────────────────────────────────────
// Backend enum: New=0, Paid=1, Sent=2, Completed=3, Cancelled=4
export type OrderStatus = 0 | 1 | 2 | 3 | 4;

export const OrderStatusLabel: Record<OrderStatus, string> = {
  0: 'New',
  1: 'Paid',
  2: 'Sent',
  3: 'Completed',
  4: 'Cancelled',
};

export const OrderStatusTransitions: Record<OrderStatus, OrderStatus[]> = {
  0: [1, 4],
  1: [2, 4],
  2: [3],
  3: [],
  4: [],
};

export interface PaymentSession {
  url: string | null;
  isPaid: boolean;
}

export interface OrderItem {
  productId: string;
  orderId: string;
  productName: string;
  productImageUrl?: string;
  quantity: number;
  price: number;
}

export interface ShippingDetails {
  fullName: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phoneNumber: string;
}

export interface Order {
  id: string;
  userId: string;
  userEmail: string;
  status: OrderStatus;
  orderItems: OrderItem[];
  totalPrice: number;
  shippingDetails: string; // JSON string — parse via JSON.parse(order.shippingDetails) as ShippingDetails
  createdAt: string;
}

// ─── Warehouse ────────────────────────────────────────────────────────────────
export interface Warehouse {
  name: string;
  createdAt: string;
}

export interface WarehouseItem {
  id: string;
  warehouseId: string;
  productId: string;
  productName: string;
  quantity: number;
}

// ─── Request models ───────────────────────────────────────────────────────────
export interface CreateProductRequest {
  name: string;
  price: number;
  category: ProductCategory;
  manufacturer?: string;
  weightGrams?: number;
}

export interface UpdateProductRequest {
  name?: string;
  price?: number;
  category?: ProductCategory;
  manufacturer?: string;
  weightGrams?: number;
}

export interface AddQuantityRequest {
  quantityToAdd: number;
}

export interface RemoveQuantityRequest {
  quantityToRemove: number;
}

// ─── Imports ──────────────────────────────────────────────────────────────────
export type ImportRunStatus = 'Queued' | 'Running' | 'Succeeded' | 'Failed';

export interface ImportRun {
  id: string;
  source: string;
  status: ImportRunStatus;
  createdAt: string;
  startedAt?: string | null;
  finishedAt?: string | null;
  added: number;
  updated: number;
  skipped: number;
  needsReview: number;
  failed: number;
  error?: string | null;
}

// ─── Builds ───────────────────────────────────────────────────────────────────
export interface BuildItem {
  productId: string;
  quantity: number;
}

export type IssueSeverity = 'Error' | 'Warning' | 'Info';

export interface CompatibilityIssue {
  severity: IssueSeverity;
  code: string;
  message: string;
  productIds: string[];
}

export interface BuildWeight {
  dryGrams: number;
  allUpGrams: number | null;
  thrustToWeight: number | null;
  isComplete: boolean;
  missingWeightProductIds: string[];
}

export interface BuildCheck {
  isValid: boolean;
  totalPrice: number;
  weight: BuildWeight;
  issues: CompatibilityIssue[];
}

export interface SavedBuildItem {
  productId: string;
  productName: string;
  category: ProductCategory;
  price: number;
  quantity: number;
  imageUrl?: string | null;
  isAvailable: boolean;
}

export interface SavedBuild {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
  totalPrice: number;
  items: SavedBuildItem[];
}
