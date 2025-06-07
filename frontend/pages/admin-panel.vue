<template>
  <NavMenu />

  <main class="p-4 md:p-8 dash" v-if="isAdmin">
    <Tabs default-value="users" class="w-full">
      <TabsList class="grid w-full grid-cols-4">
        <TabsTrigger value="users">Пользователи</TabsTrigger>
        <TabsTrigger value="products">Товары</TabsTrigger>
        <TabsTrigger value="orders">Заказы</TabsTrigger>
        <TabsTrigger value="reviews">Отзывы</TabsTrigger>
      </TabsList>

      <!-- Вкладка Пользователи -->
      <TabsContent value="users">
        <Card>
  <CardContent class="p-6 space-y-6">
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-2xl font-semibold">Пользователи</h2>
      <div class="space-x-2">
        <Button @click="openCreateUserModal" variant="default">
          + Создать пользователя
        </Button>
        <Button @click="loadUsers" variant="outline">
          <RefreshCwIcon class="w-4 h-4 mr-2" :class="{ 'animate-spin': usersLoading }" />
          Обновить
        </Button>
      </div>
    </div>

    <div v-if="usersLoading" class="flex justify-center py-8">
      <Loader2Icon class="w-8 h-8 animate-spin" />
    </div>

    <div v-else-if="usersError" class="text-red-500 text-center py-4">
      {{ usersError }}
    </div>

    <div v-else>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>ID</TableHead>
            <TableHead>Имя</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Роль</TableHead>
            <TableHead class="text-right">Действия</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <TableRow v-for="user in users" :key="user.id">
            <TableCell>{{ user.id }}</TableCell>
            <TableCell>{{ user.name || '—' }}</TableCell>
            <TableCell>{{ user.email }}</TableCell>
            <TableCell>{{ user.role || '—' }}</TableCell>
            <TableCell class="text-right space-x-2">
              <Button variant="ghost" size="sm" @click="viewUserDetails(user)">
                <EyeIcon class="w-4 h-4" />
              </Button>
              <Button variant="destructive" size="sm" @click="confirmDeleteUser(user.id)">
                <Trash2Icon class="w-4 h-4" />
              </Button>
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>
  </CardContent>
</Card>

<!-- 🧾 Модалка создания -->
<Dialog :open="createUserDialog" @close="createUserDialog = false">
  <DialogContent>
    <DialogHeader>
      <DialogTitle>Создание пользователя</DialogTitle>
    </DialogHeader>

    <div class="space-y-4">
      <!-- Роль -->
      <select v-model="newUser.role" class="w-full border rounded px-3 py-2">
        <option value="user">Обычный пользователь</option>
        <option value="seller">Продавец</option>
      </select>

      <!-- Имя (только для обычных пользователей) -->
      <Input v-model="newUser.name" placeholder="Имя"  />
      <Input v-model="newUser.email" placeholder="Email" />
      <Input v-model="newUser.password" type="password" placeholder="Пароль" />
    </div>

    <DialogFooter>
      <Button variant="outline" @click="createUserDialog = false">Отмена</Button>
      <Button @click="createUser">Создать</Button>
    </DialogFooter>
  </DialogContent>
</Dialog>

        <!-- Модальное окно деталей пользователя -->
        <Dialog v-model:open="isUserDetailsOpen">
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Детали пользователя</DialogTitle>
            </DialogHeader>
            <div class="space-y-4">
              <p><strong>ID:</strong> {{ selectedUser?.id }}</p>
              <p><strong>Имя:</strong> {{ selectedUser?.name }}</p>
              <p><strong>Email:</strong> {{ selectedUser?.email }}</p>
              <p><strong>Роль:</strong> {{ selectedUser?.role }}</p>
              <p><strong>Дата регистрации:</strong> {{ formatDate(selectedUser?.createdAt) }}</p>
              <div class="flex justify-end mt-4">
                <Button variant="outline" @click="isUserDetailsOpen = false">Закрыть</Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      </TabsContent>

      <!-- Вкладка Товары -->
      <TabsContent value="products">
        <Card>
          <CardContent class="p-6 space-y-6">
            <div class="flex justify-between items-center mb-6">
              <h2 class="text-2xl font-semibold">Управление товарами</h2>
              <Button @click="loadProducts" variant="outline">
                <RefreshCwIcon class="w-4 h-4 mr-2" :class="{ 'animate-spin': productsLoading }" />
                Обновить
              </Button>
            </div>

            <div v-if="productsLoading" class="flex justify-center py-8">
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
                      <div class="mt-2 text-lg font-semibold">{{ formatCurrency(product.price) }}</div>
                      <div class="mt-2 text-lg font-semibold">
                        <p class="card--ps">{{ product.countProduct }} шт.</p>
                      </div>
                    </div>
                  </div>

                  <div class="flex gap-2">
                    <Button variant="outline" class="flex-1" @click="startEdit(product)">Редактировать</Button>
                    <Button variant="destructive" class="flex-1"
                      @click="confirmDeleteProduct(product.id)">Удалить</Button>
                  </div>
                </CardContent>
              </Card>
            </div>
          </CardContent>
        </Card>

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

              <div class="space-y-2">
                <Label>Характеристики</Label>
                <Textarea v-model="editForm.characteristic" />
              </div>

              <div class="space-y-2">
                <Label>Количество</Label>
                <Input type="number" v-model.number="editForm.countProduct" />
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
      </TabsContent>

      <!-- Вкладка Заказы -->
      <TabsContent value="orders">
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
                            <SelectItem value="all">Все статусы</SelectItem>
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


        <!-- Модальное окно деталей заказа -->
        <Dialog v-model:open="isOrderDetailsOpen">
          <DialogContent class="sm:max-w-2xl">
            <DialogHeader>
              <DialogTitle>Заказ #{{ selectedOrder?.id }}</DialogTitle>
              <DialogDescription>
                Дата создания: {{ selectedOrder ? formatDate(selectedOrder.dateCreated) : '' }}
              </DialogDescription>
            </DialogHeader>

            <div class="space-y-2">
              <p><strong>ID:</strong> {{ selectedOrder?.id }}</p>
              <p><strong>Пользователь:</strong> {{ selectedOrder?.userId }}</p>
              <p><strong>Статус:</strong> {{ getStatusText(selectedOrder?.status) }}</p>

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

                      <div class="flex items-center gap-3 ">
                        <img v-if="item.product.images[0].path.length" :src="item.product.images[0].path"
                          class="w-12 h-12 rounded object-cover" />
                        <div>
                          <div class="width elipse">{{ item.product.name }}</div>
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

              <div class="flex justify-end gap-2 mt-4">
                <Button variant="outline" @click="isOrderDetailsOpen = false">Закрыть</Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      </TabsContent>

      <!-- Вкладка Отзывы -->
      <TabsContent value="reviews">
        <Card>
          <CardContent class="p-6 space-y-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-2xl font-semibold">Отзывы</h2>
              <Button @click="loadReviews" variant="outline">
                <RefreshCwIcon class="w-4 h-4 mr-2" :class="{ 'animate-spin': reviewsLoading }" />
                Обновить
              </Button>
            </div>

            <div v-if="reviewsLoading" class="flex justify-center py-8">
              <Loader2Icon class="w-8 h-8 animate-spin" />
            </div>

            <div v-else-if="reviewsError" class="text-red-500 text-center py-4">
              {{ reviewsError }}
            </div>

            <div v-else-if="reviews.length === 0" class="text-center py-4 text-gray-500">
              Нет отзывов
            </div>

            <div v-else class="space-y-4">
              <Card v-for="review in reviews" :key="review.id">
                <CardContent class="p-4 space-y-2">
                  <div class="flex justify-between items-center">
                    <h3 class="font-semibold">{{ review.title || 'Отзыв' }}</h3>
                    <div class="flex gap-2">
                      <Button variant="ghost" size="sm" @click="editReview(review)">
                        <Edit2Icon class="w-4 h-4" />
                      </Button>
                      <Button variant="destructive" size="sm" @click="confirmDeleteReview(review.id)">
                        <Trash2Icon class="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                  <p>{{ review.comment }}</p>
                  <p class="text-sm text-gray-500">Пользователь: {{ review.userId }}</p>
                  <router-link :to="`/product/${review.productId}`" class="text-blue-500 hover:underline">
                    <p class="text-sm text-gray-500">Товар: {{ review.productId }}</p>
                  </router-link>
                  <p class="text-sm text-gray-500">Текст отзыва: {{ review.description }}</p>
                </CardContent>
              </Card>
            </div>
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>
  </main>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  RefreshCwIcon,
  Loader2Icon,
  EyeIcon,
  Trash2Icon,
  Edit2Icon
} from 'lucide-vue-next'


const router = useRouter()

// Пользователи
const userData = ref(null)
const users = ref([])
const usersLoading = ref(false)
const usersError = ref(null)
const selectedUser = ref(null)
const isUserDetailsOpen = ref(false)

// Товары
const products = ref([])
const productsLoading = ref(false)
const isEditModalOpen = ref(false)
const editForm = reactive({
  id: null,
  name: '',
  description: '',
  characteristic: '',
  countProduct: 0,
  price: 0,
})
const productsError = ref(null)

// Заказы
const orders = ref([])
const ordersLoading = ref(false)
const ordersError = ref(null)
const selectedOrder = ref(null)
const isOrderDetailsOpen = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const isLoading = ref(true)
const createUserDialog = ref(false)
const newUser = ref({
  role: 'user',
  name: '',
  email: '',
  password: ''
})

function openCreateUserModal() {
  newUser.value = {
    role: 'user',
    name: '',
    email: '',
    password: ''
  }
  createUserDialog.value = true
}
  const config = useRuntimeConfig() 
  const apiUrl = config.public.apiBaseUrl
// ➕ Создание пользователя
async function createUser() {
  try {
    let url = ''
    let body = {}

    if (newUser.value.role === 'user') {
      url = `${apiUrl}/authuser/reg`
      body = {
        name: newUser.value.name,
        email: newUser.value.email,
        hashPassword: newUser.value.password,
        role: 1
      }
    } else {
      url = `${apiUrl}/authuser/reg`
      body = {
        name: newUser.value.name,
        email: newUser.value.email,
        hashPassword: newUser.value.password,
        role: 2
      }
    }

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: '*/*'
      },
      body: JSON.stringify(body)
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Ошибка ${response.status}: ${errorText}`)
    }

    createUserDialog.value = false
    loadUsers()
  } catch (error) {
    alert('Ошибка при создании: ' + error.message)
  }
}




const isAdmin = computed(() => {
  return userData.value?.role === 0
})

const fetchUserData = async () => {
  try {
    const response = await fetch(`${apiUrl}/user/user`, {
      method: 'GET',
      credentials: 'include'
    })

    if (!response.ok) throw new Error('Ошибка авторизации')

    userData.value = await response.json()

    // Перенаправление если не администратор
    if (userData.value.role !== 0) {
      router.push('/auth/admin-login')
      return
    }

  } catch (error) {
    console.error('Ошибка:', error)
    router.push('/auth/admin-login')
  } finally {
    isLoading.value = false
  }
}

// Отзывы
const reviews = ref([])
const reviewsLoading = ref(false)
const reviewsError = ref(null)

// Фильтрация заказов
const filteredOrders = computed(() => {
  let result = orders.value

  // Фильтр по ID заказа
  if (searchQuery.value) {
    result = result.filter(order =>
      String(order.id).includes(searchQuery.value))
  }

  // Фильтр по статусу
  if (statusFilter.value !== 'all') {
    result = result.filter(order =>
      String(order.status) === statusFilter.value
    )
  }

  return result
})

// Форматирование даты
function formatDate(dateStr) {
  if (!dateStr) return '—'
  const date = new Date(dateStr)
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
}

// Форматирование валюты
function formatCurrency(value) {
  if (typeof value !== 'number') return '—'
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB'
  }).format(value)
}

// Статусы заказов
const statusMap = {
  0: 'В сборке',
  1: 'В пути',
  2: 'Завершён',

}

function getStatusText(status) {
  return statusMap[status] || 'Неизвестно'
}

// ======= Пользователи =======
async function loadUsers() {
  usersLoading.value = true
  usersError.value = null
  try {
    const res = await fetch(`${apiUrl}/user/users`, {
      method: 'GET',
      credentials: 'include',
      headers: { 'Accept': '*/*' }
    })
    if (!res.ok) throw new Error('Ошибка загрузки пользователей')
    users.value = await res.json()
  } catch (e) {
    usersError.value = e.message || 'Ошибка загрузки пользователей'
  } finally {
    usersLoading.value = false
  }
}

function viewUserDetails(user) {
  selectedUser.value = user
  isUserDetailsOpen.value = true
}

async function confirmDeleteUser(id) {
  if (!confirm('Удалить пользователя?')) return
  try {
    const res = await fetch(`${apiUrl}/user/del/${id}`, {
      credentials: 'include',
      method: 'DELETE',
    })
    if (!res.ok) throw new Error('Ошибка удаления')
    alert('Пользователь удалён')
    await loadUsers()
  } catch (e) {
    alert(e.message || 'Ошибка удаления пользователя')
  }
}

// ======= Товары =======
async function loadProducts() {
  productsLoading.value = true
  productsError.value = null
  try {
    const res = await fetch(`${apiUrl}/product/top_product`)
    if (!res.ok) throw new Error('Ошибка сети')
    products.value = await res.json()
  } catch (e) {
    productsError.value = e.message || 'Ошибка загрузки товаров'
  } finally {
    productsLoading.value = false
  }
}

function startEdit(product) {
  editForm.id = product.id
  editForm.name = product.name
  editForm.description = product.description
  editForm.characteristic = product.characteristic
  editForm.countProduct = product.countProduct
  editForm.price = product.price
  isEditModalOpen.value = true
}

function cancelEdit() {
  isEditModalOpen.value = false
}

async function updateProduct() {
  try {
    const res = await fetch(`${apiUrl}/product/update/${editForm.id}`, {
      credentials: 'include',
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: editForm.name,
        description: editForm.description,
        characteristic: editForm.characteristic,
        countProduct: editForm.countProduct,
        price: editForm.price,
      }),
    })
    if (!res.ok) throw new Error('Ошибка обновления')
    alert('Товар обновлён')
    isEditModalOpen.value = false
    loadProducts()
  } catch (e) {
    alert(e.message || 'Ошибка обновления товара')
  }
}

async function confirmDeleteProduct(id) {
  if (!confirm('Удалить товар?')) return
  try {
    const res = await fetch(`${apiUrl}/product/delete/${id}`, {
      credentials: 'include',
      method: 'DELETE',
    })
    if (!res.ok) throw new Error('Ошибка удаления')
    alert('Товар удалён')
    loadProducts()
  } catch (e) {
    alert(e.message || 'Ошибка удаления товара')
  }
}

// ======= Заказы =======
async function loadOrders() {
  ordersLoading.value = true
  ordersError.value = null
  try {
    const res = await fetch(`${apiUrl}/order/orders`, {
      method: 'GET',
      credentials: 'include',
    })

    if (!res.ok) {
      const errorText = await res.text()
      throw new Error(`HTTP ${res.status}: ${errorText}`)
    }

    orders.value = await res.json()
  } catch (e) {
    ordersError.value = e.message || 'Ошибка загрузки заказов'
    console.error('Order load error:', e)
  } finally {
    ordersLoading.value = false
  }
}

function openOrderDetails(order) {
  selectedOrder.value = order
  isOrderDetailsOpen.value = true
}

async function confirmDeleteOrder(id) {
  if (!confirm('Удалить заказ?')) return
  try {
    const res = await fetch(`${apiUrl}/order/orders/${id}`, {
      credentials: 'include',
      method: 'DELETE',
    })
    if (!res.ok) throw new Error('Ошибка удаления')
    alert('Заказ удалён')
    loadOrders()
  } catch (e) {
    alert(e.message || 'Ошибка удаления заказа')
  }
}

async function updateOrderStatus(orderId, newStatus) {
  
  try {
    const res = await fetch(`${apiUrl}/order/orders/${orderId}?status=${newStatus}`, {
      method: 'PUT', // или 'GET', если сервер этого требует
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
// ======= Отзывы =======
async function loadReviews() {
  reviewsLoading.value = true
  reviewsError.value = null
  try {
    const res = await fetch(`${apiUrl}/review/reviews`, {
      credentials: 'include' // Добавляем передачу куки
    })
    if (!res.ok) throw new Error('Ошибка сети')
    reviews.value = await res.json()
  } catch (e) {
    reviewsError.value = e.message || 'Ошибка загрузки отзывов'
  } finally {
    reviewsLoading.value = false
  }
}

function editReview(review) {
  alert('Здесь можно открыть модальное окно редактирования отзыва (реализация по желанию)')
}

async function confirmDeleteReview(id) {
  if (!confirm('Удалить отзыв?')) return
  try {
    const res = await fetch(`${apiUrl}/review/delete_review/${id}`, {
      credentials: 'include',
      method: 'DELETE',
    })
    if (!res.ok) throw new Error('Ошибка удаления')
    alert('Отзыв удалён')
    loadReviews()
  } catch (e) {
    alert(e.message || 'Ошибка удаления отзыва')
  }
}

// Загрузка данных при монтировании
onMounted(() => {
  loadUsers()
  loadProducts()
  loadOrders()
  loadReviews()
  fetchUserData()
})
</script>


<style scoped>
.dash {
  margin-bottom: 50px;
}


/* Класс для обрезки текста с многоточием */
.elipse {
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.width {
  max-width: 150px;
}
</style>
