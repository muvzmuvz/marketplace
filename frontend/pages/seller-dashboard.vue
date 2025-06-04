<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'

import { RefreshCwIcon, Loader2Icon, EyeIcon, Trash2Icon } from 'lucide-vue-next'



interface ProductImage {
  path: string
}

interface Product {
  id: number
  name: string
  description: string
  price: number
  characteristic: string
  countProduct: number
  category: number
  images: ProductImage[]
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
interface User {
  id: number
  name: string
  email: string
  role: number
}

const form = reactive({
  name: '',
  description: '',
  characteristic: '',
  price: 0,
  countProduct: 1,
  category: 1,
  images: [] as ProductImage[]
})

const router = useRouter()
const user = ref<User | null>(null)
const isLoading = ref(true)

const isSeller = computed(() => user.value?.role === 2)

const fetchProfile = async () => {
  const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  try {
    const response = await fetch(`${apiUrl}/user/user`, {
      method: 'GET',
      credentials: 'include'
    })

    if (!response.ok) throw new Error('Ошибка авторизации')
    
    const userData = await response.json()
    user.value = userData

    // Проверка роли
    if (userData.role !== 2) {
      router.push('/')
      return
    }

  } catch (error) {
    console.error('Ошибка:', error)
    router.push('/auth/login')
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchProfile()
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

const editForm = reactive({
  name: '',
  description: '',
  price: 0,
  images: [] as ProductImage[],
  countProduct: 1,
  characteristic: '',
  category: 1
})

const isEditModalOpen = computed({
  get: () => !!editingProduct.value,
  set: (value) => {
    if (!value) editingProduct.value = null
  }
})

onMounted(() => {
  loadProducts()
  loadOrders()
})


  const config = useRuntimeConfig() 
  const apiImgUrl = config.public.apiImageUrl




async function loadProducts() {
    const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  loading.value = true
  try {
    const response = await fetch(`${apiUrl}/product/get_manager_product`, {
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
    const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  ordersLoading.value = true
  try {
    const response = await fetch(`${apiUrl}/order/OrdersOfSeller`, {
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

async function uploadImages(event: Event) {
  const input = event.target as HTMLInputElement
  const files = Array.from(input.files || [])

  if (!files.length) return

  try {
    const uploadPromises = files.map(async (file) => {
      const formData = new FormData()
      formData.append('file', file)

      const response = await fetch(`${apiImgUrl}/api/upload`, {
        method: 'POST',
        body: formData
      })

      if (!response.ok) throw new Error('Ошибка загрузки изображения')

      const data = await response.json()
      return { path: data.imagePath }  // приводим к ProductImage
    })

    const newImages = await Promise.all(uploadPromises)
    form.images.push(...newImages)
  } catch (error) {
    console.error('Ошибка загрузки изображений:', error)
    alert('❌ Не удалось загрузить некоторые изображения')
  }
}

async function submitProduct() {
    const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  if (!validateForm()) return

  try {
    const requestBody = {
      countProduct: form.countProduct,
      category: form.category,
      price: form.price,
      name: form.name,
      description: form.description,
      characteristic: form.characteristic,
      imagePath : '',
      images: form.images.map(img => ({ path: img.path }))  // только массив images
    }

    const response = await fetch(`${apiUrl}/product/create`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(requestBody),
      credentials: 'include'
    })

    if (!response.ok) throw new Error('Ошибка при создании товара')

    resetForm()
    await loadProducts()
    alert('✅ Товар успешно создан!')
  } catch (error) {
    console.error(error)
    alert('❌ Ошибка при создании товара')
  }
}


function validateForm() {
  if (!form.name.trim()) {
    alert('Введите название товара')
    return false
  }
  if (form.price <= 0) {
    alert('Цена должна быть больше нуля')
    return false
  }
  if (form.images.length === 0) {
    alert('Добавьте хотя бы одно изображение')
    return false
  }
  return true
}

function resetForm() {
  form.name = ''
  form.description = ''
  form.characteristic = ''
  form.price = 0
  form.countProduct = 1
  form.category = 1
  form.images = []
}



function removeImage(index: number) {
  form.images.splice(index, 1)
}



async function deleteProduct(id: number) {
    const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  if (confirm('Удалить товар?')) {
    await fetch(`${apiUrl}/product/delete/${id}`, {
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
  editForm.images = product.images
  editForm.count = product.countProduct
  editForm.characteristic = product.characteristic
  editForm.category = product.category
}

async function updateProduct() {
    const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  if (!editingProduct.value) return

  try {
    const requestBody = {
      name: editForm.name,
      description: editForm.description,
      price: editForm.price,
      images: editForm.images.map(img => ({ path: img.path })),
      countProduct: editForm.count,
      characteristic: editForm.characteristic,
      category: editForm.category
    }

    const response = await fetch(`${apiUrl}/product/update/${editingProduct.value.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(requestBody),
      credentials: 'include'
    })

    if (!response.ok) throw new Error('Ошибка обновления товара')

    await loadProducts()
    editingProduct.value = null
    alert('✅ Товар успешно обновлен')
  } catch (error) {
    console.error(error)
    alert('❌ Ошибка при обновлении товара')
  }
}


const filteredOrders = computed(() => {
  return orders.value.filter(order => {
    const matchesSearch = order.id.toString().includes(searchQuery.value)
    const matchesStatus = statusFilter.value === 'all' || order.status.toString() === statusFilter.value
    return matchesSearch && matchesStatus
  })
})
async function updateOrderStatus(orderId, newStatus) {
    const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  try {
    const res = await fetch(`${apiUrl}/order/orders/${orderId}?status=${newStatus}`, {
      method: 'PUT', // Оставляем PUT, если сервер требует этот метод
      credentials: 'include',
    })

    if (!res.ok) {
      const errorText = await res.text()
      throw new Error(`HTTP ${res.status}: ${errorText}`)
    }

    // Обновляем локальные данные
    const orderIndex = orders.value.findIndex(o => o.id === orderId)
    if (orderIndex !== -1) {
      orders.value[orderIndex].status = parseInt(newStatus)
    }

  } catch (e) {
    console.error('Status update error:', e)
    alert(e.message || 'Не удалось обновить статус заказа')
  }
}

function openOrderDetails(order: Order) {
  selectedOrder.value = order
  isOrderDetailsOpen.value = true
}

async function confirmDeleteOrder(id: number) {
    const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
  if (confirm('Удалить заказ?')) {
    try {
      await fetch(`${apiUrl}/order/orders/${id}`, {
        method: 'DELETE',
        credentials: 'include'
      })
      await loadOrders()
    } catch (error) {
      console.error(error)
      alert('❌ Ошибка удаления заказа')
    }
  }
}

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
    0: 'В сборке',
    1: 'В пути',
    2: 'Завершен'
  }[status] || 'Неизвестный статус'
}

function getStatusBadgeVariant(status: number) {
  return {
    0: 'secondary',
    1: 'default',
  }[status] || 'default'
}

function cancelEdit() {
  editingProduct.value = null
}
</script>


<template>
    <NavMenu />

    <main class="p-4 md:p-8 dash" v-if="isSeller">
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
                      <SelectItem value="0">В сборке</SelectItem>
                      <SelectItem value="1">В пути</SelectItem>
                      <SelectItem value="2">Завершен</SelectItem>
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
                          <Select :model-value="order.status.toString()"
                            @update:model-value="updateOrderStatus(order.id, $event)">
                            <SelectTrigger class="w-[160px]">
                              <SelectValue :placeholder="getStatusText(order.status)" />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="0">В сборке</SelectItem>
                              <SelectItem value="1">В пути</SelectItem>
                              <SelectItem value="2">Завершен</SelectItem>
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
            <CardContent class="p-6 space-y-6">
              <h2 class="text-2xl font-semibold">Создать новый товар</h2>
              <form @submit.prevent="submitProduct" class="space-y-6">
                <div class="grid gap-4 md:grid-cols-2">
                  <div class="space-y-2">
                    <Label>Название товара</Label>
                    <Input v-model="form.name" placeholder="Название" required />
                  </div>

                  <div class="space-y-2">
                    <Label>Цена (₽)</Label>
                    <Input type="number" v-model.number="form.price" required />
                  </div>

                  <div class="space-y-2">
                    <Label>Описание</Label>
                    <Textarea v-model="form.description" placeholder="Описание товара" />
                  </div>

                  <div class="space-y-2">
                    <Label>Характеристики</Label>
                    <Textarea v-model="form.characteristic" placeholder="Характеристики" />
                  </div>

                  <div class="space-y-2">
                    <Label>Категория (ID)</Label>
                    <Input type="number" v-model.number="form.category" required />
                  </div>

                  <div class="space-y-2">
                    <Label>Количество</Label>
                    <Input type="number" v-model.number="form.countProduct" required />
                  </div>
                </div>

                <div class="space-y-4">
                  <Label>Изображения товара</Label>
                  <input type="file" accept="image/*" multiple @change="uploadImages"
                    class="block w-full text-sm text-gray-500 text-hidden file:mr-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-sm file:font-semibold file:bg-primary file:text-white hover:file:bg-primary/90" />
                  <div class="flex flex-wrap gap-2 mt-2">
                    <div v-for="(image, index) in form.images" :key="index" class="relative group">
                      <img :src="image.path" class="w-32 h-32 object-cover rounded-lg border" />
                      <Button @click.prevent="removeImage(index)" variant="destructive" size="sm"
                        class="absolute top-1 right-1 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Trash2Icon class="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
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
                       <router-link :to="`/product/${product.id}`">
                      <img v-if="product.images.length" :src="product.images[0].path" :alt="product.name"
                        class="w-24 h-24 rounded-lg object-cover" />
                        </router-link>
                      <div class="flex-1 width">
                        <h3 class="font-semibold elipse">{{ product.name }}</h3>
                        <p class="text-sm text-gray-500 elipse">{{ product.description }}</p>
                        <div class="mt-2 text-lg font-semibold">
                          {{ formatCurrency(product.price) }}
                        </div>
                                                <div class="mt-2 text-lg font-semibold">
               <p class="card--ps">{{ product.countProduct }} шт.</p>
                        </div>
                      </div>
                    </div>

                    <div class="flex gap-2">
                      <Button variant="outline" class="flex-1" @click="startEdit(product)">
                        Редактировать
                      </Button>
                      <Button variant="destructive" class="flex-1" @click="deleteProduct(product.id)">
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
      <Dialog v-model:open="isEditModalOpen">
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
                        <div class="spce-y-2">
              <Label>Характеристики</Label>
              <Textarea v-model="editForm.characteristic"></Textarea>
            </div>
                        <div class="space-y-2">
              <Label>Количество</Label>
              <Input type="number" v-model.number="editForm.count" />
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

                      <div class="flex items-center gap-3 width ">
                        <img v-if="item.product.images[0].path.length" :src="item.product.images[0].path"
                          class="w-12 h-12 rounded object-cover" />
                        <div>
                          <div class=" ">{{ item.product.name }}</div>
                          <div class="text-sm text-gray-500 elipse">{{ item.product.description }}</div>
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



  
</template>



<style scoped>
.text-hidden{
  overflow: hidden;
}
.dash {
  margin-bottom: 50px;
}

.modal-overlay {
  @apply fixed inset-0 bg-black/50 flex items-center justify-center;
}

.modal {
  @apply bg-white p-6 rounded-lg max-w-md w-full;
}

.width{
  max-width: 150px;
}
.elipse{

  white-space: nowrap;       /* Не переносить текст на новую строку */
  overflow: hidden;          /* Скрыть всё, что не помещается */
  text-overflow: ellipsis;   /* Добавить "..." в конце */
}

</style>