import { useState, useEffect } from 'react';
import type { PagedResponse, ApiError } from '../types';

interface CrudPageConfig<T> {
  loadItems: (page: number, pageSize: number) => Promise<PagedResponse<T>>;
  createItem: (data: any) => Promise<T>;
  updateItem?: (id: string, data: any) => Promise<T>;
  deleteItem: (id: string) => Promise<void>;
  pageSize?: number;
}

export function useCrudPage<T extends { id: string }>(config: CrudPageConfig<T>) {
  const { loadItems, createItem, updateItem, deleteItem, pageSize = 10 } = config;

  const [items, setItems] = useState<PagedResponse<T> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<T | undefined>(undefined);
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);
  const [confirmDelete, setConfirmDelete] = useState<{ isOpen: boolean; item: T | null }>({
    isOpen: false,
    item: null,
  });

  const loadData = async (page: number = currentPage) => {
    try {
      setLoading(true);
      setError(null);
      const data = await loadItems(page, pageSize);
      setItems(data);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [currentPage]);

  const handleCreate = () => {
    setSelectedItem(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (item: T) => {
    setSelectedItem(item);
    setIsModalOpen(true);
  };

  const handleSubmit = async (data: any, successMessage?: string) => {
    try {
      setError(null);
      if (selectedItem && updateItem) {
        await updateItem(selectedItem.id, data);
        setSuccess(successMessage || 'Item updated successfully');
        // Stay on current page for updates
        loadData();
      } else {
        await createItem(data);
        setSuccess(successMessage || 'Item created successfully');
        // Reset to page 1 for new items
        setCurrentPage(1);
        await loadData(1);
      }
      setIsModalOpen(false);
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to save item');
      throw err;
    }
  };

  const handleDeleteClick = (item: T) => {
    setConfirmDelete({ isOpen: true, item });
  };

  const handleDeleteConfirm = async (successMessage?: string) => {
    if (!confirmDelete.item) return;

    try {
      setDeletingId(confirmDelete.item.id);
      setError(null);
      await deleteItem(confirmDelete.item.id);
      setSuccess(successMessage || 'Item deleted successfully');
      setConfirmDelete({ isOpen: false, item: null });
      loadData();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to delete item');
      setConfirmDelete({ isOpen: false, item: null });
    } finally {
      setDeletingId(undefined);
    }
  };

  const handleDeleteCancel = () => {
    setConfirmDelete({ isOpen: false, item: null });
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
  };

  return {
    // Data state
    items,
    loading,
    error,
    success,
    currentPage,

    // Modal state
    isModalOpen,
    selectedItem,

    // Delete state
    deletingId,
    confirmDelete,

    // Handlers
    handleCreate,
    handleEdit,
    handleSubmit,
    handleDeleteClick,
    handleDeleteConfirm,
    handleDeleteCancel,
    handlePageChange,
    handleCloseModal,
  };
}

