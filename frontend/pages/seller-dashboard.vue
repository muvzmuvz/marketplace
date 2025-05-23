<template>
  <div class="min-h-screen bg-gray-50">
    <NavMenu />
    
    <main class="p-4 md:p-8 dash">
      <Tabs default-value="crm" class="w-full">
        <TabsList class="grid w-full grid-cols-3">
          <TabsTrigger value="crm">CRM</TabsTrigger>
          <TabsTrigger value="create">Создать товар</TabsTrigger>
          <TabsTrigger value="manage">Управление товарами</TabsTrigger>
        </TabsList>

        <!-- Вкладка CRM -->
        <TabsContent value="crm">
          <Card>
            <CardContent class="p-6 space-y-6">
              <div class="flex justify-between items-center">
                <h2 class="text-2xl font-semibold">Управление заказами</h2>
                <Button @click="loadOrders" variant="outline">
                  <RefreshCwIcon class="w-4 h-4 mr-2" :class="{ 'animate-spin': ordersLoading }" />
                  Обновить
                </Button>
              </div>

              <div v-if="ordersLoading" class="flex justify-center py-8">
                <Loader2Icon class="w-8 h-8 animate-spin" />
              </div>

              <div v-else-if="ordersError" class="text-red-500 text-center py-4">
                {{ ordersError }}
              </div>

              <div v-else class="space-y-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input v-model="searchQuery" placeholder="Поиск по ID заказа" />
                  <Select v-model="statusFilter">
                    <SelectTrigger>
                      <SelectValue placeholder="Все статусы" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Все статусы</SelectItem>
                      <SelectItem value="0">Ожидает обработки</SelectItem>
                      <SelectItem value="1">В обработке</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div class="border rounded-lg overflow-hidden">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>ID</TableHead>
                        <TableHead>Пользователь</TableHead>
                        <TableHead>Товары</TableHead>
                        <TableHead>Сумма</TableHead>
                        <TableHead>Статус</TableHead>
                        <TableHead class="text-right">Действия</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      <TableRow v-for="order in filteredOrders" :key="order.id">
                        <TableCell>#{{ order.id }}</TableCell>
                        <TableCell>{{ order.userId }}</TableCell>
                        <TableCell>
                          <div v-for="item in order.products" :key="item.productId" class="text-sm">
                            {{ item.product.name }} ×{{ item.quantity }}
                          </div>
                        </TableCell>
                        <TableCell>{{ formatCurrency(order.totalPrice) }}</TableCell>
                        <TableCell>
                          <Select 
                            :model-value="order.status.toString()"
                            @update:model-value="updateOrderStatus(order.id, $event)"
                          >
                            <SelectTrigger class="w-[160px]">
                              <SelectValue :placeholder="getStatusText(order.status)" />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="0">Ожидает обработки</SelectItem>
                              <SelectItem value="1">В обработке</SelectItem>
                            </SelectContent>
                          </Select>
                        </TableCell>
                        <TableCell class="text-right space-x-2">
                          <Button variant="ghost" size="sm" @click="openOrderDetails(order)">
                            <EyeIcon class="w-4 h-4" />
                          </Button>
                          <Button variant="ghost" size="sm" class="text-red-500" @click="confirmDeleteOrder(order.id)">
                            <Trash2Icon class="w-4 h-4" />
                          </Button>
                        </TableCell>
                      </TableRow>
                    </TableBody>
                  </Table>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <!-- Вкладка создания товара -->
        <TabsContent value="create">
          <Card>
            <CardContent class="p-6">
              <h2 class="text-2xl font-semibold mb-6">Создать новый товар</h2>
              <form @submit.prevent="submitProduct" class="space-y-6">
                <div class="grid gap-4 md:grid-cols-2">
                  <div class="space-y-2">
                    <Label>Название товара</Label>
                    <Input v-model="form.Name" placeholder="Название" required />
                  </div>
                  
                  <div class="space-y-2">
                    <Label>Цена (₽)</Label>
                    <Input type="number" v-model.number="form.Price" required />
                  </div>
                  
                  <div class="space-y-2">
                    <Label>Описание</Label>
                    <Textarea v-model="form.Description" placeholder="Описание товара" />
                  </div>
                  
                  <div class="space-y-2">
                    <Label>Характеристики</Label>
                    <Textarea v-model="form.Characteristic" placeholder="Характеристики" />
                  </div>
                  
                  <div class="space-y-2">
                    <Label>Категория (ID)</Label>
                    <Input type="number" v-model.number="form.Category" required />
                  </div>
                  
                  <div class="space-y-2">
                    <Label>Количество</Label>
                    <Input type="number" v-model.number="form.CountProduct" required />
                  </div>
                </div>

                <div class="space-y-4">
                  <Label>Изображение товара</Label>
                  <input 
                    type="file" 
                    accept="image/*" 
                    @change="uploadImage" 
                    class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-sm file:font-semibold file:bg-primary file:text-white hover:file:bg-primary/90"
                  />
                  <img 
                    v-if="form.imagePath" 
                    :src="form.imagePath" 
                    class="w-32 h-32 object-cover rounded-lg border"
                  />
                </div>

                <Button type="submit" class="w-full md:w-auto">Создать товар</Button>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

        <!-- Вкладка управления товарами -->
        <TabsContent value="manage">
          <Card>
            <CardContent class="p-6">
              <div class="flex justify-between items-center mb-6">
                <h2 class="text-2xl font-semibold">Список товаров</h2>
                <Button @click="loadProducts" variant="outline">
                  <RefreshCwIcon class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
                  Обновить
                </Button>
              </div>

              <div v-if="loading" class="flex justify-center py-8">
                <Loader2Icon class="w-8 h-8 animate-spin" />
              </div>

              <div v-else-if="products.length === 0" class="text-center py-4 text-gray-500">
                Нет доступных товаров
              </div>

              <div v-else class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                <Card v-for="product in products" :key="product.id">
                  <CardContent class="p-4 space-y-4">
                    <div class="flex items-start gap-4">
                      <img 
                        :src="product.imagePath" 
                        :alt="product.name" 
                        class="w-24 h-24 rounded-lg object-cover"
                      />
                      <div class="flex-1">
                        <h3 class="font-semibold">{{ product.name }}</h3>
                        <p class="text-sm text-gray-500">{{ product.description }}</p>
                        <div class="mt-2 text-lg font-semibold">
                          {{ formatCurrency(product.price) }}
                        </div>
                      </div>
                    </div>
                    
                    <div class="flex gap-2">
                      <Button 
                        variant="outline" 
                        class="flex-1" 
                        @click="startEdit(product)"
                      >
                        Редактировать
                      </Button>
                      <Button 
                        variant="destructive" 
                        class="flex-1" 
                        @click="deleteProduct(product.id)"
                      >
                        Удалить
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <!-- Модальное окно редактирования товара -->
      <Dialog v-model:open="editingProduct">
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Редактирование товара</DialogTitle>
          </DialogHeader>
          
          <div class="space-y-4">
            <div class="space-y-2">
              <Label>Название</Label>
              <Input v-model="editForm.name" />
            </div>
            
            <div class="space-y-2">
              <Label>Описание</Label>
              <Textarea v-model="editForm.description" />
            </div>
            
            <div class="space-y-2">
              <Label>Цена</Label>
              <Input type="number" v-model.number="editForm.price" />
            </div>
            
            <div class="flex justify-end gap-2">
              <Button variant="outline" @click="cancelEdit">Отмена</Button>
              <Button @click="updateProduct">Сохранить</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <!-- Модальное окно деталей заказа -->
      <Dialog v-model:open="isOrderDetailsOpen">
        <DialogContent class="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>Заказ #{{ selectedOrder?.id }}</DialogTitle>
            <DialogDescription>
              Дата создания: {{ selectedOrder ? formatDate(selectedOrder.dateCreated) : '' }}
            </DialogDescription>
          </DialogHeader>

          <div class="grid gap-4 py-4">
            <div class="grid grid-cols-2 gap-4">
              <div class="space-y-2">
                <Label>Статус заказа</Label>
                <Badge :variant="getStatusBadgeVariant(selectedOrder?.status || 0)" class="text-sm">
                  {{ getStatusText(selectedOrder?.status || 0) }}
                </Badge>
              </div>
              
              <div class="space-y-2">
                <Label>Общая сумма</Label>
                <div class="text-xl font-semibold">
                  {{ formatCurrency(selectedOrder?.totalPrice || 0) }}
                </div>
              </div>
            </div>

            <div class="border rounded-lg overflow-hidden">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Товар</TableHead>
                    <TableHead>Цена</TableHead>
                    <TableHead>Количество</TableHead>
                    <TableHead class="text-right">Сумма</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableRow v-for="item in selectedOrder?.products" :key="item.productId">
                    <TableCell>
                      <div class="flex items-center gap-3">
                        <img 
                          :src="item.product.imagePath" 
                          class="w-12 h-12 rounded object-cover"
                        />
                        <div>
                          <div>{{ item.product.name }}</div>
                          <div class="text-sm text-gray-500">{{ item.product.description }}</div>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>{{ formatCurrency(item.price) }}</TableCell>
                    <TableCell>{{ item.quantity }}</TableCell>
                    <TableCell class="text-right">
                      {{ formatCurrency(item.price * item.quantity) }}
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </main>
  </div>
</template>

<script setup lang="ts">
import { RefreshCwIcon, Loader2Icon, EyeIcon, Trash2Icon } from 'lucide-vue-next'

interface Product {
  id: number
  name: string
  description: string
  price: number
  characteristic: string
  countProduct: number
  category: number
  imagePath: string
}

interface OrderProduct {
  productId: number
  product: Product
  quantity: number
  price: number
}

interface Order {
  id: number
  userId: number
  totalPrice: number
  status: number
  dateCreated: string
  products: OrderProduct[]
}

const form = ref({
  Name: '',
  Description: '',
  Characteristic: '',
  Price: 0,
  CountProduct: 1,
  Category: 1,
  imagePath: '',
})

const products = ref<Product[]>([])
const loading = ref(false)
const orders = ref<Order[]>([])
const ordersLoading = ref(false)
const ordersError = ref('')
const searchQuery = ref('')
const statusFilter = ref('all')
const selectedOrder = ref<Order | null>(null)
const isOrderDetailsOpen = ref(false)
const editingProduct = ref<Product | null>(null)
const orderToDelete = ref<number | null>(null)

const editForm = reactive({
  name: '',
  description: '',
  price: 0,
})

// Загрузка данных
onMounted(() => {
  loadProducts()
  loadOrders()
})

async function loadProducts() {
  loading.value = true
  try {
    const response = await fetch('http://localhost:8080/product/top_product', {
      credentials: 'include'
    })
    products.value = await response.json()
  } catch (error) {
    console.error('Ошибка загрузки товаров:', error)
  } finally {
    loading.value = false
  }
}

async function loadOrders() {
  ordersLoading.value = true
  try {
    const response = await fetch('http://localhost:8080/order/orders', {
      credentials: 'include'
    })
    orders.value = await response.json()
  } catch (error) {
    ordersError.value = 'Ошибка загрузки заказов'
    console.error(error)
  } finally {
    ordersLoading.value = false
  }
}

// Работа с товарами
async function submitProduct() {
  try {
    const response = await fetch('http://localhost:8080/product/create', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: form.value.Name,
        description: form.value.Description,
        characteristic: form.value.Characteristic,
        price: form.value.Price,
        countProduct: form.value.CountProduct,
        category: form.value.Category,
        imagePath: form.value.imagePath,
      }),
      credentials: 'include'
    })
    
    if (response.ok) {
      await loadProducts()
      form.value = {
        Name: '',
        Description: '',
        Characteristic: '',
        Price: 0,
        CountProduct: 1,
        Category: 1,
        imagePath: '',
      }
    }
  } catch (error) {
    console.error('Ошибка создания товара:', error)
  }
}

async function deleteProduct(id: number) {
  if (confirm('Удалить товар?')) {
    await fetch(`http://localhost:8080/product/delete/${id}`, {
      method: 'DELETE',
      credentials: 'include'
    })
    await loadProducts()
  }
}

function startEdit(product: Product) {
  editingProduct.value = product
  editForm.name = product.name
  editForm.description = product.description
  editForm.price = product.price
}

async function updateProduct() {
  if (!editingProduct.value) return
  
  await fetch(`http://localhost:8080/product/update/${editingProduct.value.id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(editForm),
    credentials: 'include'
  })
  
  await loadProducts()
  editingProduct.value = null
}

function cancelEdit() {
  editingProduct.value = null
}

// Работа с заказами
const filteredOrders = computed(() => {
  return orders.value.filter(order => {
    const matchesSearch = order.id.toString().includes(searchQuery.value)
    const matchesStatus = statusFilter.value === 'all' || order.status.toString() === statusFilter.value
    return matchesSearch && matchesStatus
  })
})

async function updateOrderStatus(orderId: number, status: string) {
  await fetch(`http://localhost:8080/order/orders_update/${orderId}`, {
    method: 'PUT',
    credentials: 'include'
  })
  await loadOrders()
}

function openOrderDetails(order: Order) {
  selectedOrder.value = order
  isOrderDetailsOpen.value = true
}

async function confirmDeleteOrder(id: number) {
  if (confirm('Удалить заказ?')) {
    await fetch(`http://localhost:8080/order/orders/${id}`, {
      method: 'DELETE',
      credentials: 'include'
    })
    await loadOrders()
  }
}

// Вспомогательные функции
function formatCurrency(amount: number) {
  return new Intl.NumberFormat('ru-RU', { 
    style: 'currency', 
    currency: 'RUB' 
  }).format(amount)
}

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function getStatusText(status: number) {
  return {
    0: 'Ожидает обработки',
    1: 'В обработке',
  }[status] || 'Неизвестный статус'
}

function getStatusBadgeVariant(status: number) {
  return {
    0: 'secondary',
    1: 'default',
  }[status] || 'default'
}

async function uploadImage(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await fetch('/api/upload', {
      method: 'POST',
      body: formData
    })
    const { imagePath } = await response.json()
    form.value.imagePath = imagePath
  } catch (error) {
    console.error('Ошибка загрузки изображения:', error)
  }
}
</script>

<style scoped>
.dash {
  margin-bottom: 50px;
}

.modal-overlay {
  @apply fixed inset-0 bg-black/50 flex items-center justify-center;
}

.modal {
  @apply bg-white p-6 rounded-lg max-w-md w-full;
}
</style>